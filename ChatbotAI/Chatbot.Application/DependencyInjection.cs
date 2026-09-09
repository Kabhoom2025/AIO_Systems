using Chatbot.Application.Auth;
using Chatbot.Application.Chat;
using Chatbot.Application.Common.Models;
using Chatbot.Application.Conversations;
using Chatbot.Application.Messages;
using Chatbot.Application.Settings;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatOptions>(configuration.GetSection(ChatOptions.SectionName));

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IChatOrchestrationService, ChatOrchestrationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        return services;
    }
}
