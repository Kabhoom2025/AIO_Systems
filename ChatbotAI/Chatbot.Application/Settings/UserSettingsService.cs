using Chatbot.Application.Common.Interfaces;
using Chatbot.Domain.Entities;

namespace Chatbot.Application.Settings;

public class UserSettingsService(
    IUserSettingsRepository settingsRepository,
    IApiKeyProtector apiKeyProtector) : IUserSettingsService
{
    public async Task<UserSettingsDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            await settingsRepository.AddAsync(settings, cancellationToken);
            await settingsRepository.SaveChangesAsync(cancellationToken);
        }

        return ToDto(settings);
    }

    public async Task<UserSettingsDto> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            await settingsRepository.AddAsync(settings, cancellationToken);
        }

        if (request.Language is not null) settings.Language = request.Language;
        if (request.Voice is not null) settings.Voice = request.Voice;
        if (request.SpeechRate.HasValue) settings.SpeechRate = request.SpeechRate.Value;
        if (request.Pitch.HasValue) settings.Pitch = request.Pitch.Value;
        if (request.Volume.HasValue) settings.Volume = request.Volume.Value;
        if (request.AutoSpeak.HasValue) settings.AutoSpeak = request.AutoSpeak.Value;
        if (request.VoiceMode.HasValue) settings.VoiceMode = request.VoiceMode.Value;
        if (request.Theme is not null) settings.Theme = request.Theme;

        if (request.AiProvider is not null)
        {
            settings.AiProvider = request.AiProvider.Length == 0 ? null : request.AiProvider;
        }

        if (request.AiApiKey is not null)
        {
            settings.AiApiKeyEncrypted = request.AiApiKey.Length == 0 ? null : apiKeyProtector.Protect(request.AiApiKey);
        }

        if (request.AiModel is not null)
        {
            settings.AiModel = request.AiModel.Length == 0 ? null : request.AiModel;
        }

        await settingsRepository.SaveChangesAsync(cancellationToken);

        return ToDto(settings);
    }

    private static UserSettingsDto ToDto(UserSettings settings) => new(
        settings.Language, settings.Voice, settings.SpeechRate, settings.Pitch,
        settings.Volume, settings.AutoSpeak, settings.VoiceMode, settings.Theme,
        settings.AiProvider, !string.IsNullOrEmpty(settings.AiApiKeyEncrypted), settings.AiModel);
}
