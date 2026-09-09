namespace Chatbot.Application.Settings;

public interface IUserSettingsService
{
    Task<UserSettingsDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSettingsDto> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken = default);
}
