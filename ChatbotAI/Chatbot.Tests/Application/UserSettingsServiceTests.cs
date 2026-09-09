using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Settings;
using Chatbot.Domain.Entities;
using Moq;
using Xunit;

namespace Chatbot.Tests.Application;

public class UserSettingsServiceTests
{
    private readonly Mock<IUserSettingsRepository> _settingsRepository = new();
    private readonly Mock<IApiKeyProtector> _apiKeyProtector = new();
    private readonly UserSettingsService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public UserSettingsServiceTests()
    {
        _sut = new UserSettingsService(_settingsRepository.Object, _apiKeyProtector.Object);
    }

    [Fact]
    public async Task UpdateAsync_EncryptsAndStoresApiKey_WhenProvided()
    {
        var settings = new UserSettings { UserId = _userId };
        _settingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _apiKeyProtector.Setup(p => p.Protect("xai-secret-key")).Returns("PROTECTED(xai-secret-key)");

        var result = await _sut.UpdateAsync(_userId, new UpdateUserSettingsRequest(
            null, null, null, null, null, null, null, null, "Grok", "xai-secret-key", null));

        Assert.Equal("PROTECTED(xai-secret-key)", settings.AiApiKeyEncrypted);
        Assert.Equal("Grok", settings.AiProvider);
        Assert.True(result.HasCustomAiApiKey);
        Assert.Equal("Grok", result.AiProvider);
    }

    [Fact]
    public async Task UpdateAsync_ClearsApiKeyAndProvider_WhenEmptyStringsProvided()
    {
        var settings = new UserSettings { UserId = _userId, AiProvider = "Grok", AiApiKeyEncrypted = "PROTECTED(old)" };
        _settingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        var result = await _sut.UpdateAsync(_userId, new UpdateUserSettingsRequest(
            null, null, null, null, null, null, null, null, "", "", null));

        Assert.Null(settings.AiProvider);
        Assert.Null(settings.AiApiKeyEncrypted);
        Assert.False(result.HasCustomAiApiKey);
        Assert.Null(result.AiProvider);
        _apiKeyProtector.Verify(p => p.Protect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_LeavesApiKeyUnchanged_WhenFieldNotProvided()
    {
        var settings = new UserSettings { UserId = _userId, AiApiKeyEncrypted = "PROTECTED(existing)" };
        _settingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        var result = await _sut.UpdateAsync(_userId, new UpdateUserSettingsRequest(
            "en-GB", null, null, null, null, null, null, null, null, null, null));

        Assert.Equal("PROTECTED(existing)", settings.AiApiKeyEncrypted);
        Assert.True(result.HasCustomAiApiKey);
        Assert.Equal("en-GB", settings.Language);
    }

    [Fact]
    public async Task UpdateAsync_SetsAndClearsTheModelOverride()
    {
        var settings = new UserSettings { UserId = _userId };
        _settingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        var setResult = await _sut.UpdateAsync(_userId, new UpdateUserSettingsRequest(
            null, null, null, null, null, null, null, null, null, null, "openai/gpt-oss-120b"));
        Assert.Equal("openai/gpt-oss-120b", settings.AiModel);
        Assert.Equal("openai/gpt-oss-120b", setResult.AiModel);

        var clearResult = await _sut.UpdateAsync(_userId, new UpdateUserSettingsRequest(
            null, null, null, null, null, null, null, null, null, null, ""));
        Assert.Null(settings.AiModel);
        Assert.Null(clearResult.AiModel);
    }

    [Fact]
    public async Task GetAsync_NeverExposesTheRawOrEncryptedKey()
    {
        var settings = new UserSettings { UserId = _userId, AiApiKeyEncrypted = "PROTECTED(secret)" };
        _settingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        var result = await _sut.GetAsync(_userId);

        Assert.True(result.HasCustomAiApiKey);
        var dtoJson = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.DoesNotContain("PROTECTED", dtoJson);
        Assert.DoesNotContain("secret", dtoJson);
    }
}
