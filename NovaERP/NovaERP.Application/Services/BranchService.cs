using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateBranchDto> _createValidator;
    private readonly IValidator<UpdateBranchDto> _updateValidator;

    public BranchService(IBranchRepository repo, IUnitOfWork unitOfWork, IMapper mapper,
        IValidator<CreateBranchDto> createValidator, IValidator<UpdateBranchDto> updateValidator)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<BranchDto>> GetAllAsync(int orgId)
    {
        var branches = await _repo.GetAllByOrgAsync(orgId);
        return branches.Select(_mapper.Map<BranchDto>).ToList();
    }

    public async Task<BranchDto> GetByIdAsync(int orgId, int id)
    {
        var branch = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> CreateAsync(int orgId, CreateBranchDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var branch = _mapper.Map<Branch>(dto);
        branch.OrganizationId = orgId;
        branch.IsActive = true;

        _repo.Add(branch);
        // Demonstrates the thin IUnitOfWork abstraction in addition to the repository's own SaveChangesAsync.
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> UpdateAsync(int orgId, int id, UpdateBranchDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var branch = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");

        _mapper.Map(dto, branch);
        branch.UpdatedDate = DateTime.UtcNow;

        _repo.Update(branch);
        await _repo.SaveChangesAsync();
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var branch = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Branch {id} not found");
        _repo.Remove(branch);
        await _repo.SaveChangesAsync();
    }
}
