using AutoMapper;
using FoodOrder.Application.DTOs.Settings;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public SettingsService(ISettingsRepository settingsRepository, IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public async Task<SettingsDto> GetSettingsAsync()
    {
        var settings = await _settingsRepository.GetSettingsAsync()
            ?? throw new AppException("Restaurant settings have not been configured.", statusCode: 404);

        return _mapper.Map<SettingsDto>(settings);
    }

    public async Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsDto dto)
    {
        var settings = await _settingsRepository.GetSettingsAsync()
            ?? throw new AppException("Restaurant settings have not been configured.", statusCode: 404);

        _mapper.Map(dto, settings);
        _settingsRepository.Update(settings);
        await _settingsRepository.SaveChangesAsync();

        return _mapper.Map<SettingsDto>(settings);
    }

    public async Task<SettingsDto> GetOrgSettingsAsync(int orgId)
    {
        var settings = await _settingsRepository.GetByOrgAsync(orgId)
            ?? throw new AppException("Settings not found.", statusCode: 404);
        return _mapper.Map<SettingsDto>(settings);
    }

    public async Task<SettingsDto> UpsertOrgSettingsAsync(int orgId, UpdateSettingsDto dto)
    {
        var existing = await _settingsRepository.GetByOrgAsync(orgId);

        if (existing == null || existing.OrganizationId == null)
        {
            // Create a new org-specific settings row cloned from the incoming dto
            var orgSettings = _mapper.Map<Settings>(dto);
            orgSettings.OrganizationId = orgId;
            await _settingsRepository.AddAsync(orgSettings);
            await _settingsRepository.SaveChangesAsync();
            return _mapper.Map<SettingsDto>(orgSettings);
        }

        _mapper.Map(dto, existing);
        _settingsRepository.Update(existing);
        await _settingsRepository.SaveChangesAsync();
        return _mapper.Map<SettingsDto>(existing);
    }
}
