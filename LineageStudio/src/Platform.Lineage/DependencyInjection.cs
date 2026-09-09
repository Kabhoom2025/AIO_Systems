using Microsoft.Extensions.DependencyInjection;
using Platform.Lineage.Execution;
using Platform.Lineage.Query;
using Platform.Lineage.Recording;
using Platform.Runtime.Execution;

namespace Platform.Lineage;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformLineage(this IServiceCollection services)
    {
        services.AddSingleton<MetadataMasker>();
        services.AddSingleton<ILineageEventPublisher, NullLineageEventPublisher>();
        services.AddScoped<ILineageEngine, LineageEngine>();
        services.AddScoped<ILineageQueryService, LineageQueryService>();

        // Overrides the plain IRuntimeExecutionService registration from AddPlatformRuntime -
        // last registration wins for constructor injection - so every caller asking for
        // IRuntimeExecutionService transparently gets lineage-tracked execution.
        services.AddScoped<IRuntimeExecutionService, LineageTrackedRuntimeExecutionService>();

        return services;
    }
}
