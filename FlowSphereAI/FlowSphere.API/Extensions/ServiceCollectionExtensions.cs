using System.Reflection;
using FlowSphere.API.Hubs;
using FlowSphere.Application.Auth.Commands.Login;
using FlowSphere.Application.Behaviors;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Copilot;
using FlowSphere.Execution.Engine;
using FlowSphere.Execution.Nodes;
using FlowSphere.Execution.Queue;
using FlowSphere.Infrastructure.Authentication;
using FlowSphere.Infrastructure.Data;
using FlowSphere.Infrastructure.Seed;
using FlowSphere.Infrastructure.Persistence;
using FluentValidation;
using Hangfire;
using Hangfire.Redis.StackExchange;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FlowSphere.API.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Assembly ApplicationAssembly = typeof(LoginCommand).Assembly;

    public static IServiceCollection AddFlowSphereApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(ApplicationAssembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(WorkspacePermissionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(ApplicationAssembly);

        return services;
    }

    public static IServiceCollection AddFlowSphereInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<ClientSettings>(configuration.GetSection(ClientSettings.SectionName));

        services.AddDbContext<FlowSphereDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();

            // EF Core's PendingModelChangesWarning compares the live model against the last
            // migration's snapshot, but TenantModelCacheKeyFactory intentionally makes the
            // compiled model vary per-tenant (that's the whole point of the per-tenant query
            // filter) - this makes the comparison a persistent false positive, confirmed by
            // generating a migration immediately after this one and finding an empty diff.
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<FlowSphereDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<ICurrentEnvironmentContext, CurrentEnvironmentContext>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<DemoDataSeeder>();
        services.AddScoped<IConnectorCredentialStore, ConnectorCredentialStore>();
        services.AddScoped<IAppNotificationEmailSender, FlowSphere.Infrastructure.Email.AppNotificationEmailSender>();
        services.AddScoped<IAppOtpSmsSender, FlowSphere.Infrastructure.Notifications.AppOtpSmsSender>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<IAppNotificationDispatcher, FlowSphere.Infrastructure.Notifications.AppNotificationDispatcher>();
        services.AddScoped<IAppTriggerDispatcher, FlowSphere.Infrastructure.Notifications.AppTriggerDispatcher>();
        services.AddDataProtection();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "FlowSphereAI:";
        });

        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionCatalog.All)
            {
                options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }

    public static IServiceCollection AddFlowSphereExecution(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("FlowSphereConnectors");

        var redisConnectionString = configuration.GetConnectionString("Redis")!;

        // Redis-backed storage makes queued executions durable (survive an API restart) and lets
        // multiple API instances share one job queue - a real distributed-dispatch upgrade over
        // the earlier in-process Channel<Guid>, which lost anything still queued on restart and
        // could only ever be drained by the single process that enqueued it.
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(redisConnectionString));
        services.AddHangfireServer();

        services.AddScoped<WorkflowExecutionJob>();
        services.AddScoped<IExecutionQueue, HangfireExecutionQueue>();

        services.AddScoped<IExecutionNotifier, SignalRExecutionNotifier>();

        services.AddScoped<IConnector, HttpConnector>();
        services.AddScoped<IConnector, SmtpConnector>();
        services.AddScoped<IConnector, OpenAiConnector>();
        services.AddScoped<IConnector, SlackConnector>();
        services.AddScoped<IConnector, TeamsConnector>();
        services.AddScoped<IConnector, DiscordConnector>();
        services.AddScoped<IConnector, WebhookConnector>();
        services.AddScoped<IConnector, TwilioConnector>();
        services.AddScoped<IConnector, NotionConnector>();
        services.AddScoped<IConnector, JiraConnector>();
        services.AddScoped<IConnector, AirtableConnector>();
        services.AddScoped<IConnector, GoogleSheetsConnector>();
        services.AddScoped<ConnectorRegistry>();

        services.AddScoped<INodeExecutor, TriggerWebhookNodeExecutor>();
        services.AddScoped<INodeExecutor, HttpRequestNodeExecutor>();
        services.AddScoped<INodeExecutor, ConditionNodeExecutor>();
        services.AddScoped<INodeExecutor, DelayNodeExecutor>();
        services.AddScoped<INodeExecutor, EmailSmtpNodeExecutor>();
        services.AddScoped<INodeExecutor, AiPromptNodeExecutor>();
        services.AddScoped<INodeExecutor, SlackMessageNodeExecutor>();
        services.AddScoped<INodeExecutor, TeamsMessageNodeExecutor>();
        services.AddScoped<INodeExecutor, DiscordMessageNodeExecutor>();
        services.AddScoped<INodeExecutor, DecisionNodeExecutor>();
        services.AddScoped<INodeExecutor, MergeNodeExecutor>();
        services.AddScoped<INodeExecutor, ExceptionNodeExecutor>();
        services.AddScoped<INodeExecutor, WebhookNodeExecutor>();
        services.AddScoped<INodeExecutor, TwilioSmsNodeExecutor>();
        services.AddScoped<INodeExecutor, NotionCreatePageNodeExecutor>();
        services.AddScoped<INodeExecutor, JiraCreateIssueNodeExecutor>();
        services.AddScoped<INodeExecutor, AirtableCreateRecordNodeExecutor>();
        services.AddScoped<INodeExecutor, GoogleSheetsAppendRowNodeExecutor>();
        // Loop, Parallel, and UserTask are handled directly by WorkflowExecutionEngine (they
        // control the walk itself - iterate/fan-out/suspend - rather than producing a single
        // NodeResult), so they deliberately have no INodeExecutor registration.
        services.AddScoped<INodeExecutorRegistry, NodeExecutorRegistry>();

        services.AddScoped<WorkflowExecutionEngine>();
        services.AddScoped<IWorkflowGraphGenerator, WorkflowGraphGenerator>();
        services.AddScoped<IAppFormGenerator, AppFormGenerator>();

        return services;
    }
}
