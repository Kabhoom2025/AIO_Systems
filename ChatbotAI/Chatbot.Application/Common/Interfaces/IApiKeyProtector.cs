namespace Chatbot.Application.Common.Interfaces;

/// <summary>Encrypts/decrypts a user-supplied AI provider API key for storage at rest.</summary>
public interface IApiKeyProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedText);
}
