using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Conversations;
using Chatbot.Domain.Entities;
using Moq;
using Xunit;

namespace Chatbot.Tests.Application;

public class ConversationServiceTests
{
    private readonly Mock<IConversationRepository> _conversationRepository = new();
    private readonly ConversationService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public ConversationServiceTests()
    {
        _sut = new ConversationService(_conversationRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_DefaultsTitle_WhenNoneProvided()
    {
        Conversation? captured = null;
        _conversationRepository.Setup(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Callback<Conversation, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(_userId, new CreateConversationRequest(null));

        Assert.Equal("New Chat", result.Title);
        Assert.NotNull(captured);
        Assert.Equal(_userId, captured!.UserId);
    }

    [Fact]
    public async Task GetDetailAsync_ThrowsNotFound_WhenConversationDoesNotBelongToUser()
    {
        _conversationRepository.Setup(r => r.GetWithMessagesAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetDetailAsync(_userId, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_RemovesConversation_WhenOwnedByUser()
    {
        var conversation = new Conversation { UserId = _userId };
        _conversationRepository.Setup(r => r.GetByIdAsync(conversation.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.DeleteAsync(_userId, conversation.Id);

        _conversationRepository.Verify(r => r.Remove(conversation), Times.Once);
        _conversationRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTitleAndArchivedFlag()
    {
        var conversation = new Conversation { UserId = _userId, Title = "Old title", IsArchived = false };
        _conversationRepository.Setup(r => r.GetByIdAsync(conversation.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var result = await _sut.UpdateAsync(_userId, conversation.Id, new UpdateConversationRequest("New title", true));

        Assert.Equal("New title", result.Title);
        Assert.True(result.IsArchived);
    }
}
