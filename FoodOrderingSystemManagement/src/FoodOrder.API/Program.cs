using FoodOrder.API.Extensions;
using FoodOrder.API.Hubs;
using FoodOrder.API.Middlewares;
using FoodOrder.Infrastructure.Data;
using FoodOrder.Infrastructure.Data.SeedData;
using FoodOrder.Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSingleton<EventLogService>();

var app = builder.Build();

// ── Database: apply pending migrations and seed reference data ────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger    = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var eventLog  = scope.ServiceProvider.GetRequiredService<EventLogService>();
    await DbSeeder.SeedAsync(dbContext, logger);
    eventLog.Log("INFO", "API server started successfully.", "Startup");
}

// ── Middleware pipeline (order is significant) ────────────────────────────────

// 1. Correlation ID — must be first so every downstream middleware/log has it.
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Exception handler — must be early so it catches errors from all subsequent middleware.
app.UseMiddleware<ExceptionMiddleware>();

// 3. Request logging — after exception middleware so the final status code is accurate.
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. Swagger — development only.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Order API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root: https://localhost:{port}/
    });
}

// 5. HTTPS redirect.
app.UseHttpsRedirection();

// 6. CORS — must come before Authentication/Authorization.
app.UseCors("FoodOrderCorsPolicy");

// 7. Authentication & Authorization.
app.UseAuthentication();
app.UseAuthorization();

// 8. License enforcement — runs after auth so claims are populated.
//    Blocks org users whose license is missing or expired.
app.UseMiddleware<LicenseMiddleware>();

// 10. Controllers.
app.MapControllers();

// 11. Health check endpoint — useful for load balancer and container orchestration probes.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHub<OrderHub>("/hubs/orders");

app.Run();
