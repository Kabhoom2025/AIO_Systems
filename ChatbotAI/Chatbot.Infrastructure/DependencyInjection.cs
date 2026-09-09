using Chatbot.Application.Common.Interfaces;
using Chatbot.Infrastructure.Ai;
using Chatbot.Infrastructure.Authentication;
using Chatbot.Infrastructure.Persistence;
using Chatbot.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IApiKeyProtector, DataProtectionApiKeyProtector>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();

        AddAiProvider(services);

        return services;
    }

    private static void AddAiProvider(IServiceCollection services)
    {
        // Named clients for every provider (not just the server-configured default) so
        // IAiChatServiceFactory can build any of them on demand for a user's "bring your own
        // key" override, with the same timeout the default path gets.
        services.AddHttpClient(nameof(OpenAiChatService), client => client.Timeout = TimeSpan.FromMinutes(5));
        services.AddHttpClient(nameof(GrokChatService), client => client.Timeout = TimeSpan.FromMinutes(5));
        services.AddHttpClient(nameof(GroqChatService), client => client.Timeout = TimeSpan.FromMinutes(5));
        services.AddHttpClient(nameof(AzureOpenAiChatService), client => client.Timeout = TimeSpan.FromMinutes(5));

        services.AddScoped<IAiChatServiceFactory, AiChatServiceFactory>();
        services.AddScoped<IAiChatService>(sp => sp.GetRequiredService<IAiChatServiceFactory>().Create(null, null));
    }
}
