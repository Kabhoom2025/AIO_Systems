using Chatbot.Application.Chat;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Common.Models;
using Chatbot.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Chatbot.Tests.Application;

public class ChatOrchestrationServiceTests
{
    private readonly Mock<IConversationRepository> _conversationRepository = new();
    private readonly Mock<IMessageRepository> _messageRepository = new();
    private readonly Mock<IUserSettingsRepository> _userSettingsRepository = new();
    private readonly Mock<IAiChatServiceFactory> _aiChatServiceFactory = new();
    private readonly Mock<IApiKeyProtector> _apiKeyProtector = new();
    private readonly Mock<IAiChatService> _resolvedAiService = new();
    private readonly ChatOrchestrationService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public ChatOrchestrationServiceTests()
    {
        _sut = new ChatOrchestrationService(
            _conversationRepository.Object,
            _messageRepository.Object,
            _userSettingsRepository.Object,
            _aiChatServiceFactory.Object,
            _apiKeyProtector.Object,
            Options.Create(new ChatOptions()));
    }

    [Fact]
    public async Task ResolveAiServiceAsync_UsesServerDefaults_WhenUserHasNoCustomSettings()
    {
        _userSettingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSettings?)null);
        _aiChatServiceFactory.Setup(f => f.Create(null, null, null)).Returns(_resolvedAiService.Object);

        var result = await _sut.ResolveAiServiceAsync(_userId);

        Assert.Same(_resolvedAiService.Object, result);
        _apiKeyProtector.Verify(p => p.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAiServiceAsync_DecryptsAndPassesTheUsersOwnProviderAndKey()
    {
        var settings = new UserSettings { UserId = _userId, AiProvider = "Grok", AiApiKeyEncrypted = "PROTECTED(xai-key)" };
        _userSettingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _apiKeyProtector.Setup(p => p.Unprotect("PROTECTED(xai-key)")).Returns("xai-key");
        _aiChatServiceFactory.Setup(f => f.Create("Grok", "xai-key", null)).Returns(_resolvedAiService.Object);

        var result = await _sut.ResolveAiServiceAsync(_userId);

        Assert.Same(_resolvedAiService.Object, result);
        _aiChatServiceFactory.Verify(f => f.Create("Grok", "xai-key", null), Times.Once);
    }

    [Fact]
    public async Task ResolveAiServiceAsync_FallsBackToServerDefaultKey_WhenUserSetProviderButNoKey()
    {
        var settings = new UserSettings { UserId = _userId, AiProvider = "OpenAI", AiApiKeyEncrypted = null };
        _userSettingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _aiChatServiceFactory.Setup(f => f.Create("OpenAI", null, null)).Returns(_resolvedAiService.Object);

        var result = await _sut.ResolveAiServiceAsync(_userId);

        Assert.Same(_resolvedAiService.Object, result);
        _apiKeyProtector.Verify(p => p.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAiServiceAsync_PassesTheUsersModelOverrideToTheFactory()
    {
        var settings = new UserSettings { UserId = _userId, AiProvider = "Groq", AiModel = "openai/gpt-oss-120b" };
        _userSettingsRepository.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _aiChatServiceFactory.Setup(f => f.Create("Groq", null, "openai/gpt-oss-120b")).Returns(_resolvedAiService.Object);

        var result = await _sut.ResolveAiServiceAsync(_userId);

        Assert.Same(_resolvedAiService.Object, result);
        _aiChatServiceFactory.Verify(f => f.Create("Groq", null, "openai/gpt-oss-120b"), Times.Once);
    }

    [Fact]
    public async Task PrepareTurnAsync_IncludesTheJustSentMessage_InTheHistoryHandedToTheAi()
    {
        // Regression test: history must be read *after* the new user message is persisted,
        // otherwise the AI never sees the message it's meant to reply to and instead answers
        // based on context ending one turn earlier ("replying to the previous message").
        var conversation = new Conversation { UserId = _userId };
        _conversationRepository.Setup(r => r.GetByIdAsync(conversation.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var addedMessages = new List<Message>();
        _messageRepository.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => addedMessages.Add(m))
            .Returns(Task.CompletedTask);

        // Reflects whatever has actually been added by the time this is called — if the
        // implementation fetches history before saving the user message, this list (and thus
        // the assertion below) will be empty.
        _messageRepository.Setup(r => r.GetRecentHistoryAsync(conversation.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => addedMessages.ToList());

        var prepared = await _sut.PrepareTurnAsync(_userId, conversation.Id, "What's the capital of France?");

        Assert.Contains(prepared.History, m => m.Role == "user" && m.Content == "What's the capital of France?");
    }
}
