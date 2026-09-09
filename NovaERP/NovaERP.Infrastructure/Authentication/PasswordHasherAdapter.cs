using NovaERP.Application.Interfaces;

namespace NovaERP.Infrastructure.Authentication;

public class PasswordHasherAdapter : IPasswordHasher
{
    public string Hash(string password) => PasswordHasher.Hash(password);
    public bool Verify(string password, string stored) => PasswordHasher.Verify(password, stored);
}
