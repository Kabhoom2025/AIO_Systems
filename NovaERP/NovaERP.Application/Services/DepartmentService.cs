using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDepartmentDto> _createValidator;
    private readonly IValidator<UpdateDepartmentDto> _updateValidator;

    public DepartmentService(IDepartmentRepository repo, IMapper mapper,
        IValidator<CreateDepartmentDto> createValidator, IValidator<UpdateDepartmentDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<DepartmentDto>> GetAllAsync(int orgId)
    {
        var departments = await _repo.GetAllByOrgAsync(orgId);
        return departments.Select(_mapper.Map<DepartmentDto>).ToList();
    }

    public async Task<DepartmentDto> GetByIdAsync(int orgId, int id)
    {
        var department = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Department {id} not found");
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> CreateAsync(int orgId, CreateDepartmentDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var department = _mapper.Map<Department>(dto);
        department.OrganizationId = orgId;
        department.IsActive = true;

        _repo.Add(department);
        await _repo.SaveChangesAsync();
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> UpdateAsync(int orgId, int id, UpdateDepartmentDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var department = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Department {id} not found");

        _mapper.Map(dto, department);
        department.UpdatedDate = DateTime.UtcNow;

        _repo.Update(department);
        await _repo.SaveChangesAsync();
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var department = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Department {id} not found");
        _repo.Remove(department);
        await _repo.SaveChangesAsync();
    }
}
