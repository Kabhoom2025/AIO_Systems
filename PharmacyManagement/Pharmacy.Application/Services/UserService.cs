using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo) => _repo = repo;

    public async Task<List<UserSummaryDto>> GetAllAsync(int orgId)
    {
        var users = await _repo.GetAllByOrgAsync(orgId);
        return users.Select(MapToDto).ToList();
    }

    private static UserSummaryDto MapToDto(User u) => new()
    {
        Id       = u.Id,
        Name     = u.Name,
        Email    = u.Email,
        RoleName = u.Role?.Name ?? string.Empty,
        BranchId = u.BranchId
    };
}
