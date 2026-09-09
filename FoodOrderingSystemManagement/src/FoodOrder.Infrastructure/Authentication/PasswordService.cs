using FoodOrder.Application.Interfaces.Services;

namespace FoodOrder.Infrastructure.Authentication;

/// <summary>
/// BCrypt implementation with work factor 12 — balances security and hashing speed.
/// Work factor 12 takes ~300ms per hash on modern hardware, which is acceptable
/// for a login endpoint but makes brute-force attacks computationally expensive.
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool VerifyPassword(string plainText, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainText, hash);
}
