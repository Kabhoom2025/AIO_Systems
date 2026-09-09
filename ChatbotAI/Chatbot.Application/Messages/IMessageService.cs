namespace Chatbot.Application.Messages;

public interface IMessageService
{
    Task DeleteAsync(Guid userId, Guid messageId, CancellationToken cancellationToken = default);
}
