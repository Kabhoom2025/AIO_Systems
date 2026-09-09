using AutoMapper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repo;
    private readonly IMapper _mapper;

    public PermissionService(IPermissionRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<PermissionDto>> GetAllAsync()
    {
        var permissions = await _repo.GetAllAsync();
        return permissions.Select(_mapper.Map<PermissionDto>).ToList();
    }
}
