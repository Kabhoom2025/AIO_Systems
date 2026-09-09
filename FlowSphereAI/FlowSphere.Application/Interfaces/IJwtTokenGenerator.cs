using FlowSphere.Domain.Entities;

namespace FlowSphere.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, Role role);
}
