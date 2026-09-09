using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _repo;
    private readonly IValidator<CreateContactDto> _createValidator;
    private readonly IValidator<UpdateContactDto> _updateValidator;

    public ContactService(IContactRepository repo,
        IValidator<CreateContactDto> createValidator, IValidator<UpdateContactDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ContactDto>> GetAllAsync(int orgId)
    {
        var contacts = await _repo.GetAllByOrgAsync(orgId);
        return contacts.Select(ToDto).ToList();
    }

    public async Task<ContactDto> GetByIdAsync(int orgId, int id)
    {
        var contact = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Contact {id} not found");
        return ToDto(contact);
    }

    public async Task<ContactDto> CreateAsync(int orgId, CreateContactDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var contact = new Contact
        {
            OrganizationId = orgId,
            AccountId = dto.AccountId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Title = dto.Title,
            OwnerId = dto.OwnerId
        };

        _repo.Add(contact);
        await _repo.SaveChangesAsync();
        return ToDto(contact);
    }

    public async Task<ContactDto> UpdateAsync(int orgId, int id, UpdateContactDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var contact = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Contact {id} not found");

        contact.AccountId = dto.AccountId;
        contact.FirstName = dto.FirstName;
        contact.LastName = dto.LastName;
        contact.Email = dto.Email;
        contact.Phone = dto.Phone;
        contact.Title = dto.Title;
        contact.OwnerId = dto.OwnerId;
        contact.UpdatedDate = DateTime.UtcNow;

        _repo.Update(contact);
        await _repo.SaveChangesAsync();
        return ToDto(contact);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var contact = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Contact {id} not found");
        _repo.Remove(contact);
        await _repo.SaveChangesAsync();
    }

    private static ContactDto ToDto(Contact c) => new()
    {
        Id = c.Id,
        AccountId = c.AccountId,
        AccountName = c.Account?.Name,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        Phone = c.Phone,
        Title = c.Title,
        OwnerId = c.OwnerId,
        OwnerName = c.Owner?.Name ?? string.Empty
    };
}
