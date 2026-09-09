using FoodOrder.Application.DTOs;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IOrganizationRepository
{
    Task<IEnumerable<Organization>> GetAllAsync();
    Task<Organization?> GetByIdAsync(int id);
    Task<Organization> CreateAsync(Organization org);
    Task UpdateAsync(Organization org);
    Task DeleteAsync(int id);
    Task<int> GetUserCountAsync(int organizationId);
    Task<IEnumerable<User>> GetOrgUsersAsync(int organizationId);
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<OrgReportDto> GetOrgReportAsync(int orgId);
}
