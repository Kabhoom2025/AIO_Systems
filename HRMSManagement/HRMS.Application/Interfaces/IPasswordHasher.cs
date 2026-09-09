namespace HRMS.Application.Interfaces;

/// <summary>Abstraction so Application-layer services can hash passwords without referencing Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string stored);
}
