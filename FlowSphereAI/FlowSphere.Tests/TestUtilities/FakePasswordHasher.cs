using FlowSphere.Application.Interfaces;

namespace FlowSphere.Tests.TestUtilities;

/// <summary>Trivial reversible "hash" (prefixed plain text) - fast and deterministic for tests,
/// never used outside the test project.</summary>
public class FakePasswordHasher : IPasswordHasher
{
    private const string Prefix = "hashed:";

    public string Hash(string password) => Prefix + password;

    public bool Verify(string password, string hash) => hash == Prefix + password;
}
