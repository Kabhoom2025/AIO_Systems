using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Messages;

namespace Chatbot.Application.Chat;

public record SendMessageRequest(Guid? ConversationId, string Message);

public record RegenerateRequest(Guid MessageId);

public record ChatTurnResult(Guid ConversationId, MessageDto UserMessage, MessageDto AssistantMessage);

public record PreparedTurn(Guid ConversationId, MessageDto? UserMessage, IReadOnlyCollection<ChatMessage> History);
