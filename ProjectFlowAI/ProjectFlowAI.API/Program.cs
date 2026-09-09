using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectFlowAI.API.Authentication;
using ProjectFlowAI.API.Filters;
using ProjectFlowAI.API.Hubs;
using ProjectFlowAI.API.Middleware;
using ProjectFlowAI.API.Realtime;
using ProjectFlowAI.Application.Behaviors;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Application.Features.Notifications;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Infrastructure.Data;
using ProjectFlowAI.Infrastructure.Jobs;
using ProjectFlowAI.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog — structured console logging configured from appsettings, mirroring NovaERP.API's setup.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Database — the "Testing" environment (WebApplicationFactory in ProjectFlowAI.Tests) always gets
// an isolated EF Core InMemory database instead, so integration tests never need a real Postgres server.
var testingDbName = Guid.NewGuid().ToString();
builder.Services.AddDbContext<ProjectFlowDbContext>(opts =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        opts.UseInMemoryDatabase(testingDbName);
    else
        opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IProjectFlowDbContext>(sp => sp.GetRequiredService<ProjectFlowDbContext>());

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Application services (Interfaces -> Infrastructure implementations)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ITotpService, TotpService>();
builder.Services.AddScoped<IOAuthVerifier, OAuthVerifier>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Phase 4: Chat / Notifications / Documents — channel senders degrade to logging when
// unconfigured (mirrors SmtpEmailSender), realtime notifiers are backed by the SignalR hubs
// mapped below, and NotificationDispatcher orchestrates all of it.
builder.Services.AddScoped<ISlackNotifier, SlackNotifier>();
builder.Services.AddScoped<ITeamsNotifier, TeamsNotifier>();
builder.Services.AddScoped<ISmsNotifier, SmsNotifier>();
builder.Services.AddScoped<IChatRealtimeNotifier, ChatHubNotifier>();
builder.Services.AddScoped<INotificationRealtimeNotifier, NotificationsHubNotifier>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddSignalR();

// Phase 6: AI Features / Workflow Automation. AnthropicAiClient degrades gracefully — IsConfigured
// is false whenever Ai:ApiKey is empty, and every AI command handler checks that before attempting
// any call (see AiNotConfiguredException, mapped to 503 in GlobalExceptionMiddleware). WorkflowEngine
// is the cross-cutting service wired into WorkItem/Sprint command handlers (mirrors
// INotificationDispatcher's wiring style); HangfireWorkflowScheduler backs the Scheduled trigger type.
builder.Services.AddSingleton<IAiClient, AnthropicAiClient>();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();
builder.Services.AddScoped<IWorkflowScheduler, HangfireWorkflowScheduler>();
builder.Services.AddScoped<ScheduledWorkflowJob>();

// Hangfire — same pattern as NovaERP.API: own "hangfire" schema/tables in the same Postgres
// database, own recurring-job registrar (HangfireWorkflowScheduler / ScheduledWorkflowJob), and a
// "Testing" environment fallback to in-memory storage so WebApplicationFactory-based integration
// tests never need a real database or a running server.
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings();

    if (builder.Environment.IsEnvironment("Testing"))
        cfg.UseMemoryStorage();
    else
#pragma warning disable CS0618
        cfg.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
#pragma warning restore CS0618
});
builder.Services.AddHangfireServer();

// Local-disk attachment storage under {ContentRoot}/App_Data/attachments (Phase 2: WorkItem attachments).
var attachmentsBasePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "attachments");
builder.Services.AddSingleton<IFileStorageService>(_ => new FileStorageService(attachmentsBasePath));

// MediatR + FluentValidation + AutoMapper — CQRS stack for the whole Application assembly.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PermissionCatalog).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(PermissionCatalog).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

// JWT — this app's own secret/issuer/audience (ProjectFlow AI is a standalone service with its own login).
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
        // Browser WebSocket connections can't set an Authorization header, so the SignalR client
        // sends the JWT as a query param instead — only honored on /hubs/* paths (mirrors
        // NovaERP.API/FlowSphere.API's identical OnMessageReceived pattern).
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionCatalog.All)
        options.AddPolicy(permission.Key, p => p.Requirements.Add(new PermissionRequirement(permission.Key)));
});

// Rate limiting — fixed window on the Auth endpoints (login/register/forgot-password), keyed per client IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueLimit = 0
            }));
});

// Enums serialize/deserialize as their string names (e.g. "InProgress", not 1) everywhere in the
// API — the frontend's TypeScript contract for WorkItemStatus/Priority/Type/etc. is written
// against string values, and integers would silently desync from that the moment any enum's
// member order changes.
builder.Services.AddControllers(options => options.Filters.Add<AuditLogActionFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectFlow AI API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Bearer token, e.g. \"Bearer {token}\"",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProjectFlowAICors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("ProjectFlowAICors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ProjectFlowAI.API", timeUtc = DateTime.UtcNow }));

// Hub paths must stay exactly "/hubs/chat" and "/hubs/notifications" (not "/hubs/projectflow/*")
// so the ApiGateway's pre-wired "/hubs/projectflow/{**catch-all}" -> "/hubs/{**catch-all}" forward
// reaches them correctly.
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationsHub>("/hubs/notifications");

// Apply pending EF Core migrations + seed demo data on startup in dev (mirrors every other sibling
// service). Uses Migrate() rather than EnsureCreated() so schema changes across phases (e.g. the
// Phase 2 Projects/WorkItems tables added after the DB already existed) actually get applied.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
    db.Database.Migrate();
    await SeedData.SeedAsync(db);

    // Re-register Hangfire recurring jobs for every currently-enabled Scheduled workflow, so a
    // restarted app doesn't lose its schedules (mirrors NovaERP's RecurringJobRegistrar-on-startup pattern).
    var scheduler = scope.ServiceProvider.GetRequiredService<IWorkflowScheduler>();
    var scheduledWorkflows = await db.WorkflowDefinitions
        .Where(w => w.IsEnabled && w.TriggerType == WorkflowTriggerType.Scheduled)
        .ToListAsync();
    foreach (var workflow in scheduledWorkflows)
        scheduler.RegisterOrUpdate(workflow.Id, workflow.TriggerConfigJson);

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
    });
}

app.Run();

/// <summary>Exposed so WebApplicationFactory&lt;Program&gt; can be used in integration tests.</summary>
public partial class Program { }
