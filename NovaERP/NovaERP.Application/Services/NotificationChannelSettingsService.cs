using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class NotificationChannelSettingsService : INotificationChannelSettingsService
{
    private readonly INotificationChannelSettingsRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateNotificationChannelSettingsDto> _updateValidator;

    public NotificationChannelSettingsService(INotificationChannelSettingsRepository repo, IMapper mapper,
        IValidator<UpdateNotificationChannelSettingsDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _updateValidator = updateValidator;
    }

    public async Task<NotificationChannelSettingsDto> GetAsync(int orgId)
    {
        var settings = await _repo.GetByOrgAsync(orgId);
        // No row yet is the normal default state for a freshly-created org — return an empty
        // (all-null) DTO rather than 404, so the settings screen can render a blank form.
        if (settings == null)
            return new NotificationChannelSettingsDto { OrganizationId = orgId, SmtpUseSsl = true };

        return _mapper.Map<NotificationChannelSettingsDto>(settings);
    }

    public async Task<NotificationChannelSettingsDto> UpdateAsync(int orgId, UpdateNotificationChannelSettingsDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var settings = await _repo.GetByOrgAsync(orgId);
        if (settings == null)
        {
            settings = new NotificationChannelSettings { OrganizationId = orgId };
            _mapper.Map(dto, settings);
            _repo.Add(settings);
        }
        else
        {
            _mapper.Map(dto, settings);
            settings.UpdatedDate = DateTime.UtcNow;
            _repo.Update(settings);
        }

        await _repo.SaveChangesAsync();
        return _mapper.Map<NotificationChannelSettingsDto>(settings);
    }
}
