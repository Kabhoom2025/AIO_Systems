using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Domain.Entities;

namespace Chatbot.Application.Messages;

public class MessageService(IMessageRepository messageRepository) : IMessageService
{
    public async Task DeleteAsync(Guid userId, Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await messageRepository.GetByIdAsync(messageId, cancellationToken);
        if (message is null || message.Conversation is null || message.Conversation.UserId != userId)
        {
            throw new NotFoundException(nameof(Message), messageId);
        }

        message.IsDeleted = true;
        await messageRepository.SaveChangesAsync(cancellationToken);
    }
}
