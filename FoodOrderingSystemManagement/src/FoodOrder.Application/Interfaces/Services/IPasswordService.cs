namespace FoodOrder.Application.Interfaces.Services;

/// <summary>
/// Abstracts BCrypt so services stay testable and the hashing algorithm
/// can be swapped without touching business logic.
/// </summary>
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string plainText, string hash);
}
