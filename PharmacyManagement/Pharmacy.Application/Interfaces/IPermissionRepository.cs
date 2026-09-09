using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();
}
