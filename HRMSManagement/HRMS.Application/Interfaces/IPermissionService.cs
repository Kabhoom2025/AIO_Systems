using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllAsync();
}
