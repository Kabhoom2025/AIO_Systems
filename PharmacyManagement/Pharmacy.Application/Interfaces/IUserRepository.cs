using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllByOrgAsync(int orgId);
}
