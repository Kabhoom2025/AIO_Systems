using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateOrganizationDto> _updateValidator;

    public OrganizationService(IOrganizationRepository repo, IMapper mapper,
        IValidator<UpdateOrganizationDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _updateValidator = updateValidator;
    }

    public async Task<OrganizationDto> GetProfileAsync(int orgId)
    {
        var org = await _repo.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException($"Organization {orgId} not found");
        return _mapper.Map<OrganizationDto>(org);
    }

    public async Task<OrganizationDto> UpdateProfileAsync(int orgId, UpdateOrganizationDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var org = await _repo.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException($"Organization {orgId} not found");

        _mapper.Map(dto, org);
        org.UpdatedDate = DateTime.UtcNow;

        _repo.Update(org);
        await _repo.SaveChangesAsync();
        return _mapper.Map<OrganizationDto>(org);
    }
}
