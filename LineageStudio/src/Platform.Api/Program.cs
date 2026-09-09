using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Platform.Api.Hubs;
using Platform.Api.Middleware;
using Platform.Infrastructure;
using Platform.Infrastructure.Persistence;
using Platform.Lineage;
using Platform.Lineage.Recording;
using Platform.Runtime;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddPlatformInfrastructure(builder.Configuration);
builder.Services.AddPlatformRuntime();
builder.Services.AddPlatformLineage();

// Overrides the no-op ILineageEventPublisher registered by AddPlatformLineage - last
// registration wins for constructor injection - so lineage recording also broadcasts live
// over SignalR without Platform.Lineage needing to reference ASP.NET Core's SignalR types.
builder.Services.AddSingleton<ILineageEventPublisher, SignalRLineageEventPublisher>();

var jwt = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
        };
        options.Events = new JwtBearerEvents
        {
            // Browsers can't attach an Authorization header to the WebSocket upgrade request
            // SignalR's JS client makes, so the token travels as a query param instead for hub
            // paths specifically - never accepted this way for ordinary API requests.
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/lineage"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddSignalR()
    .AddJsonProtocol(opts => opts.PayloadSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LineageStudioCors", policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    db.Database.Migrate();

    // Disabled for the integration test host (ApiFactory sets Seed:Enabled=false) - tests reset
    // the database themselves and shouldn't pay for or depend on demo data.
    if (builder.Configuration.GetValue("Seed:Enabled", true))
        await SeedData.SeedAsync(scope.ServiceProvider);
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("LineageStudioCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LineageHub>("/hubs/lineage");
app.MapGet("/", () => Results.Ok(new { service = "Platform.Api", status = "Healthy", timeUtc = DateTime.UtcNow }));

app.Run();

public partial class Program { }
