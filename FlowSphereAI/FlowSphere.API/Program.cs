using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FlowSphere.API.Extensions;
using FlowSphere.API.HealthChecks;
using FlowSphere.API.Hubs;
using FlowSphere.API.Middleware;
using FlowSphere.Infrastructure.Data;
using FlowSphere.Infrastructure.Seed;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console());

// Without this, System.Text.Json's default enum binding expects an integer, not a name like
// "WorkspaceAdmin" - request bodies with an enum-typed property (e.g. AssignAdminRequest.Role)
// would 400 on every string value a client naturally sends. Response bodies already convert
// enums to strings manually via .ToString() in each handler, so this is purely an input-side fix.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis backplane: execution/notification events broadcast from any API instance reach clients
// connected to any other instance - required once this runs as more than one replica.
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis")!);

builder.Services.AddFlowSphereApplication();
builder.Services.AddFlowSphereInfrastructure(builder.Configuration);
builder.Services.AddFlowSphereExecution(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };

        // Browser WebSocket connections can't set an Authorization header, so the SignalR
        // client sends the token as a query param instead - only honored on the hub path.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:4206", "http://localhost:5000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Per-IP throttling on auth endpoints (login/register) - unauthenticated, so IP is the only
// identity available before a token exists. Blocks brute-force/spam-registration without
// touching the shared ApiGateway's config (which every other module also routes through).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { code = "RateLimited", message = "Too many attempts. Please wait a moment and try again." },
            cancellationToken);
    };
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Dev-only: the dashboard is a plain browser page with its own cookie-based auth model,
    // which doesn't line up with this API's bearer-JWT-in-localStorage auth. A production
    // deployment needs a real auth story for it (e.g. a dedicated admin login, or restricting
    // it at the reverse-proxy/network level) - deliberately not faked here with a filter that
    // wouldn't actually protect anything.
    app.MapHangfireDashboard("/hangfire");
}

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ExecutionHub>("/hubs/execution");
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowSphereDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();

public partial class Program { }
