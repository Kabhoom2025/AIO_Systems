using Chatbot.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Chatbot.Infrastructure.Authentication;

/// <summary>Encrypts a user's own AI provider API key at rest using ASP.NET Core Data Protection.</summary>
public class DataProtectionApiKeyProtector : IApiKeyProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionApiKeyProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Chatbot.UserSettings.AiApiKey.v1");
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedText) => _protector.Unprotect(protectedText);
}
