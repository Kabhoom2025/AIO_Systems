using Microsoft.Extensions.DependencyInjection;
using Platform.Runtime.Execution;

namespace Platform.Runtime;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformRuntime(this IServiceCollection services)
    {
        // Registered under its concrete type too, so Platform.Lineage's decorator can depend on
        // "the real runtime engine" directly without asking DI for IRuntimeExecutionService
        // (which it itself replaces) and causing a resolution cycle.
        services.AddScoped<RuntimeExecutionService>();
        services.AddScoped<IRuntimeExecutionService>(sp => sp.GetRequiredService<RuntimeExecutionService>());
        return services;
    }
}
