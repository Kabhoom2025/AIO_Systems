using System.Net.Sockets;
using LinkShield.Application.Interfaces;
using LinkShield.Application.Services;
using LinkShield.Infrastructure.Analysis;
using LinkShield.Infrastructure.Authentication;
using LinkShield.Infrastructure.BackgroundProcessing;
using LinkShield.Infrastructure.Notifications;
using LinkShield.Infrastructure.Persistence;
using LinkShield.Infrastructure.Risk;
using LinkShield.Infrastructure.Security;
using LinkShield.Infrastructure.Services;
using LinkShield.Infrastructure.ThreatIntelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace LinkShield.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLinkShieldInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<LinkShieldDbContext>(opts =>
        {
            if (environment.IsEnvironment("Testing"))
                opts.UseInMemoryDatabase(Guid.NewGuid().ToString());
            else
                opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!environment.IsEnvironment("Testing") && !string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = redisConnection;
                opts.InstanceName = "LinkShield:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IUrlAnalyzer, UrlAnalyzer>();
        services.AddScoped<IScanService, ScanService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IBrandManagementService, BrandManagementService>();
        services.AddScoped<IRiskRuleManagementService, RiskRuleManagementService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IApiClientService, ApiClientService>();
        services.AddSingleton<IApiKeyRateLimiter, LinkShield.Infrastructure.ApiKeyAuth.InMemoryApiKeyRateLimiter>();
        services.AddScoped<IApiKeyValidator, LinkShield.Infrastructure.ApiKeyAuth.ApiKeyValidator>();

        // SSRF protection — every HttpClient below that talks to a user-influenced host routes
        // its socket connections through SsrfSafeConnector via ConnectCallback. See docs/security.md.
        services.AddSingleton<ISsrfGuard, SsrfGuard>();
        services.AddSingleton<ISsrfSafeConnector, SsrfSafeConnector>();

        var requestTimeoutMs = int.TryParse(configuration["SsrfProtection:RequestTimeoutMs"], out var t) ? t : 5000;

        services.AddHttpClient(DomainAnalyzer.HttpClientName, client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs);
            })
            .ConfigurePrimaryHttpMessageHandler(sp => BuildSsrfSafeHandler(sp, allowAutoRedirect: true))
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(200 * attempt)));

        services.AddHttpClient(RedirectAnalyzer.HttpClientName, client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs);
            })
            .ConfigurePrimaryHttpMessageHandler(sp => BuildSsrfSafeHandler(sp, allowAutoRedirect: false));

        services.AddSingleton<IDnsAnalyzer, DnsAnalyzer>();
        services.AddScoped<IDomainAnalyzer, DomainAnalyzer>();
        services.AddScoped<ISslAnalyzer, SslAnalyzer>();
        services.AddScoped<IRedirectAnalyzer, RedirectAnalyzer>();

        // Threat intelligence — each provider is its own HttpClient (public API hosts, not
        // user-influenced, so no SSRF connect callback needed here) with a short retry policy.
        services.AddHttpClient(UrlhausProvider.HttpClientName, c => c.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs))
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(300)));
        services.AddHttpClient(PhishTankProvider.HttpClientName, c => c.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs))
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(300)));
        services.AddHttpClient(GoogleSafeBrowsingProvider.HttpClientName, c => c.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs))
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(300)));
        services.AddHttpClient(VirusTotalProvider.HttpClientName, c => c.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs))
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(300)));

        services.AddScoped<IThreatIntelligenceProvider, UrlhausProvider>();
        services.AddScoped<IThreatIntelligenceProvider, PhishTankProvider>();
        services.AddScoped<IThreatIntelligenceProvider, GoogleSafeBrowsingProvider>();
        services.AddScoped<IThreatIntelligenceProvider, VirusTotalProvider>();
        services.AddScoped<IThreatIntelligenceService, ThreatIntelligenceService>();

        services.AddScoped<IBrandDetectionService, BrandDetectionService>();
        services.AddScoped<IRiskEngine, RiskEngine>();

        var mlServiceBaseUrl = configuration["MlService:BaseUrl"] ?? "http://localhost:8001";
        var mlServiceTimeoutMs = int.TryParse(configuration["MlService:TimeoutMs"], out var mt) ? mt : 3000;
        services.AddHttpClient(MlPredictionClient.HttpClientName, c =>
        {
            c.BaseAddress = new Uri(mlServiceBaseUrl);
            c.Timeout = TimeSpan.FromMilliseconds(mlServiceTimeoutMs);
        });
        services.AddScoped<IMlPredictionClient, MlPredictionClient>();

        // Background scan processing — see IScanPipelineRunner. IScanNotifier defaults to a
        // no-op here; LinkShield.API registers a SignalR-backed one after calling this method,
        // which wins for constructor injection (last registration wins for a single service).
        services.AddSingleton<IScanQueue, ScanQueue>();
        services.AddSingleton<IScanNotifier, NullScanNotifier>();
        services.AddScoped<IScanPipelineRunner, ScanPipelineRunner>();

        return services;
    }

    private static System.Net.Http.SocketsHttpHandler BuildSsrfSafeHandler(IServiceProvider sp, bool allowAutoRedirect)
    {
        var connector = sp.GetRequiredService<ISsrfSafeConnector>();
        return new System.Net.Http.SocketsHttpHandler
        {
            AllowAutoRedirect = allowAutoRedirect,
            ConnectCallback = async (context, ct) =>
            {
                var socket = await connector.ConnectAsync(context.DnsEndPoint.Host, context.DnsEndPoint.Port, ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
        };
    }
}
