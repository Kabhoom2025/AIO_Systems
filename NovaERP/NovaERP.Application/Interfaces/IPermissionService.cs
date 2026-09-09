using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllAsync();
}
