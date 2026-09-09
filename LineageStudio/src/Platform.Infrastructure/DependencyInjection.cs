using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.Auth;
using Platform.Application.DataDesigner;
using Platform.Application.ImpactAnalysis;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Infrastructure.ApiDesigner;
using Platform.Infrastructure.Applications;
using Platform.Infrastructure.Auth;
using Platform.Infrastructure.DataDesigner;
using Platform.Infrastructure.ImpactAnalysis;
using Platform.Infrastructure.MappingDesigner;
using Platform.Infrastructure.Persistence;
using Platform.Infrastructure.UiBuilder;

namespace Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IDataTableService, DataTableService>();
        services.AddScoped<IScreenService, ScreenService>();
        services.AddScoped<IComponentService, ComponentService>();
        services.AddScoped<IServiceDefinitionService, ServiceDefinitionService>();
        services.AddScoped<IApiEndpointService, ApiEndpointService>();
        services.AddScoped<IMappingService, MappingService>();
        services.AddScoped<IImpactAnalysisService, ImpactAnalysisService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
