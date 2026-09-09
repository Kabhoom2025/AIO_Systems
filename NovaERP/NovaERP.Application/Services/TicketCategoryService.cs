using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class TicketCategoryService : ITicketCategoryService
{
    private readonly ITicketCategoryRepository _repo;
    private readonly IValidator<CreateTicketCategoryDto> _createValidator;
    private readonly IValidator<UpdateTicketCategoryDto> _updateValidator;

    public TicketCategoryService(ITicketCategoryRepository repo,
        IValidator<CreateTicketCategoryDto> createValidator, IValidator<UpdateTicketCategoryDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<TicketCategoryDto>> GetAllAsync(int orgId)
    {
        var categories = await _repo.GetAllByOrgAsync(orgId);
        return categories.Select(ToDto).ToList();
    }

    public async Task<TicketCategoryDto> GetByIdAsync(int orgId, int id)
    {
        var category = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"TicketCategory {id} not found");
        return ToDto(category);
    }

    public async Task<TicketCategoryDto> CreateAsync(int orgId, CreateTicketCategoryDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Ticket category code {code} already exists");

        var category = new TicketCategory
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            IsActive = dto.IsActive
        };

        _repo.Add(category);
        await _repo.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task<TicketCategoryDto> UpdateAsync(int orgId, int id, UpdateTicketCategoryDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var category = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"TicketCategory {id} not found");

        category.Name = dto.Name;
        category.IsActive = dto.IsActive;
        category.UpdatedDate = DateTime.UtcNow;

        _repo.Update(category);
        await _repo.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var category = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"TicketCategory {id} not found");
        _repo.Remove(category);
        await _repo.SaveChangesAsync();
    }

    private static TicketCategoryDto ToDto(TicketCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Code = c.Code,
        IsActive = c.IsActive
    };
}
