using System.Text;
using FluentValidation;
using LinkShield.API.Authentication;
using LinkShield.API.BackgroundServices;
using LinkShield.API.Hubs;
using LinkShield.API.Middleware;
using LinkShield.API.Notifications;
using LinkShield.Application.Interfaces;
using LinkShield.Application.Validators.Auth;
using LinkShield.Infrastructure;
using LinkShield.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId());

builder.Services.AddLinkShieldInfrastructure(builder.Configuration, builder.Environment);

// Overrides the no-op IScanNotifier registered by AddLinkShieldInfrastructure — last
// registration wins for constructor injection, so ScanPipelineRunner (Infrastructure) gets
// this SignalR-backed implementation without Infrastructure needing to reference the API's Hub.
builder.Services.AddScoped<IScanNotifier, SignalRScanNotifier>();
builder.Services.AddHostedService<ScanProcessingHostedService>();

var jwt = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/scan-progress"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.SchemeName, _ => { });

builder.Services.AddAuthorization();
builder.Services.AddValidatorsFromAssembly(typeof(RegisterRequestValidator).Assembly);
builder.Services.AddControllers()
    .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LinkShieldCors", policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials());
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

// Brute-force throttling on the auth endpoints most worth abusing (login/register/refresh/
// forgot-password) — partitioned per client IP, 10 requests/minute, no queueing (429
// immediately once exceeded rather than making the caller wait). In-process only, same
// single-instance caveat as InMemoryApiKeyRateLimiter — revisit with a distributed limiter if
// LinkShield.API ever runs more than one replica.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("LinkShieldCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ScanProgressHub>("/hubs/scan-progress");
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "LinkShield.API", status = "Healthy", timeUtc = DateTime.UtcNow }));

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LinkShieldDbContext>();
    db.Database.EnsureCreated();
    await SeedData.SeedAsync(db);
}

app.Run();

public partial class Program { }
