using LinkShield.Application.Interfaces;

namespace LinkShield.Infrastructure.Authentication;

public class PasswordHasherAdapter : IPasswordHasher
{
    public string Hash(string password) => PasswordHasher.Hash(password);
    public bool Verify(string password, string storedHash) => PasswordHasher.Verify(password, storedHash);
}
