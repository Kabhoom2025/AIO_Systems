using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Users.Queries.GetOrganizationUsers;

public record GetOrganizationUsersQuery : IRequest<Result<IReadOnlyList<OrganizationUserDto>>>;

/// <summary>Status is computed, not stored: "Pending" while email is unverified or no role is
/// assigned yet, else "Active"/"Inactive" from IsActive - matches Quixy's activation model.</summary>
public record OrganizationUserDto(
    int Id,
    string Name,
    string Email,
    string? RoleName,
    bool IsActive,
    bool IsEmailVerified,
    string? ManagerName,
    string Status,
    DateTime? LastLoginAt);
