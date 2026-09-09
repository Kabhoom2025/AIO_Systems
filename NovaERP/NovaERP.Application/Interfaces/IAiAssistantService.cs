using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAiAssistantService
{
    Task<List<ConversationSummaryDto>> GetAllAsync(int orgId, int userId);
    Task<ConversationDto> GetByIdAsync(int orgId, int userId, int id);
    Task<ConversationDto> CreateAsync(int orgId, int userId, CreateConversationDto dto);
    Task DeleteAsync(int orgId, int userId, int id);
    Task<ConversationDto> SendMessageAsync(int orgId, int userId, int conversationId, SendMessageDto dto, bool canCreateServiceTickets);
    Task<ConversationDto> ConfirmActionAsync(int orgId, int userId, int conversationId, int messageId);
    Task<ConversationDto> CancelActionAsync(int orgId, int userId, int conversationId, int messageId);
}
