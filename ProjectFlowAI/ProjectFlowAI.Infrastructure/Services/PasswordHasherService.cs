using Microsoft.AspNetCore.Identity;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Services;

/// <summary>Thin wrapper over ASP.NET Core Identity's PasswordHasher&lt;T&gt; — we don't pull in
/// the full Identity framework, just its battle-tested PBKDF2 hashing implementation.</summary>
public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string password) => _hasher.HashPassword(new User(), password);

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword)) return false;
        var result = _hasher.VerifyHashedPassword(new User(), hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
