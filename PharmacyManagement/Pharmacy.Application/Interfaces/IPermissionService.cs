using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllAsync();
}
