using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;

namespace FlowSphere.Tests.TestUtilities;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public string GenerateToken(User user, Role role) => $"fake-token-for-{user.Email}";
}
