using System.Text.Json;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

/// <summary>A Conversation belongs to a User, not a Role — same self-service, ownership-guard
/// shape as DashboardService. The one read-only tool available (get_widget_data, via the
/// existing IWidgetDataService) is deliberately restricted to foundation-module data for the
/// same "permission-free surface must not expose sensitive data through a back door" reasoning
/// Dashboard Builder already established. A second tool, propose_create_service_ticket, is only
/// ever offered to the model when the caller has the real "service-desk.create" permission
/// (computed by AiAssistantController via ASP.NET's own authorization service, passed in as
/// AiToolContext) — and even then it never auto-executes: it stops the turn and persists a
/// Pending action that a human must explicitly confirm via ConfirmActionAsync before anything
/// is actually created. When no AI provider key is configured, no HTTP call is ever attempted —
/// a friendly "not configured" reply is persisted instead.</summary>
public class AiAssistantService : IAiAssistantService
{
    private const int MaxToolCallIterations = 3;
    private static readonly string[] ValidPriorities = { "Low", "Medium", "High", "Critical" };

    private readonly IConversationRepository _repo;
    private readonly IAiProvider _aiProvider;
    private readonly IWidgetDataService _widgetDataService;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ITicketCategoryService _ticketCategoryService;
    private readonly IServiceTicketService _serviceTicketService;

    public AiAssistantService(IConversationRepository repo, IAiProvider aiProvider,
        IWidgetDataService widgetDataService, IEmployeeRepository employeeRepo,
        ITicketCategoryService ticketCategoryService, IServiceTicketService serviceTicketService)
    {
        _repo = repo;
        _aiProvider = aiProvider;
        _widgetDataService = widgetDataService;
        _employeeRepo = employeeRepo;
        _ticketCategoryService = ticketCategoryService;
        _serviceTicketService = serviceTicketService;
    }

    public async Task<List<ConversationSummaryDto>> GetAllAsync(int orgId, int userId)
    {
        var conversations = await _repo.GetAllByUserAsync(orgId, userId);
        return conversations.Select(ToSummaryDto).ToList();
    }

    public async Task<ConversationDto> GetByIdAsync(int orgId, int userId, int id)
    {
        var conversation = await GetOwnedAsync(orgId, userId, id);
        return ToDto(conversation);
    }

    public async Task<ConversationDto> CreateAsync(int orgId, int userId, CreateConversationDto dto)
    {
        var conversation = new Conversation
        {
            OrganizationId = orgId,
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(dto.Title) ? "New Conversation" : dto.Title
        };

        _repo.Add(conversation);
        await _repo.SaveChangesAsync();
        return ToDto(conversation);
    }

    public async Task DeleteAsync(int orgId, int userId, int id)
    {
        var conversation = await GetOwnedAsync(orgId, userId, id);
        _repo.Remove(conversation);
        await _repo.SaveChangesAsync();
    }

    public async Task<ConversationDto> SendMessageAsync(int orgId, int userId, int conversationId, SendMessageDto dto, bool canCreateServiceTickets)
    {
        var conversation = await GetOwnedAsync(orgId, userId, conversationId);

        var userMessage = new Message { ConversationId = conversation.Id, Role = "user", Content = dto.Content };
        _repo.AddMessage(userMessage);
        await _repo.SaveChangesAsync();

        if (!await _aiProvider.IsConfiguredAsync(orgId))
        {
            _repo.AddMessage(new Message
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = "AI Assistant isn't configured yet — ask an administrator to set an API key."
            });
            await _repo.SaveChangesAsync();

            return ToDto(await _repo.GetByIdAsync(orgId, conversation.Id) ?? conversation);
        }

        var toolContext = new AiToolContext { CanCreateServiceTickets = canCreateServiceTickets };

        var turns = conversation.Messages
            .Where(m => m.Role is "user" or "assistant")
            .OrderBy(m => m.CreatedDate)
            .Select(m => new AiChatTurn { Role = m.Role, Content = m.Content })
            .ToList();
        turns.Add(new AiChatTurn { Role = "user", Content = dto.Content });

        string? finalText = null;
        for (var i = 0; i < MaxToolCallIterations && finalText is null; i++)
        {
            var result = await _aiProvider.CompleteAsync(orgId, turns, toolContext);

            if (result.FinalText is not null)
            {
                finalText = result.FinalText;
                break;
            }

            if (result.ToolCallName is null) break;

            if (result.ToolCallName == "propose_create_service_ticket" && canCreateServiceTickets)
            {
                var proposal = await ProposeCreateServiceTicketAsync(orgId, userId, result.ToolCallArgumentsJson);
                proposal.ConversationId = conversation.Id;
                _repo.AddMessage(proposal);
                await _repo.SaveChangesAsync();
                return ToDto(await _repo.GetByIdAsync(orgId, conversation.Id) ?? conversation);
            }

            if (result.ToolCallName == "get_widget_data")
            {
                var widgetType = ParseWidgetType(result.ToolCallArgumentsJson);
                if (widgetType is null) break;

                turns.Add(new AiChatTurn { Role = "assistant_tool_use", ToolName = "get_widget_data", Content = result.ToolCallArgumentsJson ?? "{}", ToolCallId = result.ToolCallId });

                var widgetData = await _widgetDataService.GetDataAsync(orgId, widgetType);
                var widgetDataJson = JsonSerializer.Serialize(widgetData);

                _repo.AddMessage(new Message { ConversationId = conversation.Id, Role = "tool", Content = widgetDataJson });
                turns.Add(new AiChatTurn { Role = "tool", Content = widgetDataJson, ToolCallId = result.ToolCallId });
                continue;
            }

            break;
        }

        _repo.AddMessage(new Message
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = finalText ?? "I wasn't able to complete that request."
        });
        await _repo.SaveChangesAsync();

        var final = await _repo.GetByIdAsync(orgId, conversation.Id) ?? conversation;
        return ToDto(final);
    }

    public async Task<ConversationDto> ConfirmActionAsync(int orgId, int userId, int conversationId, int messageId)
    {
        var conversation = await GetOwnedAsync(orgId, userId, conversationId);
        var message = GetPendingMessage(conversation, messageId);

        if (message.PendingActionType == "CreateServiceTicket")
        {
            var createDto = JsonSerializer.Deserialize<CreateServiceTicketDto>(message.PendingActionPayloadJson!)!;
            var ticket = await _serviceTicketService.CreateAsync(orgId, createDto);

            message.PendingActionStatus = "Confirmed";
            _repo.AddMessage(new Message
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = $"Done — created Service Ticket {ticket.TicketNumber}: \"{ticket.Subject}\"."
            });
        }

        await _repo.SaveChangesAsync();
        return ToDto(await _repo.GetByIdAsync(orgId, conversation.Id) ?? conversation);
    }

    public async Task<ConversationDto> CancelActionAsync(int orgId, int userId, int conversationId, int messageId)
    {
        var conversation = await GetOwnedAsync(orgId, userId, conversationId);
        var message = GetPendingMessage(conversation, messageId);

        message.PendingActionStatus = "Cancelled";
        _repo.AddMessage(new Message
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = "Okay, I won't create that."
        });

        await _repo.SaveChangesAsync();
        return ToDto(await _repo.GetByIdAsync(orgId, conversation.Id) ?? conversation);
    }

    private static Message GetPendingMessage(Conversation conversation, int messageId)
    {
        var message = conversation.Messages.FirstOrDefault(m => m.Id == messageId)
            ?? throw new KeyNotFoundException($"Message {messageId} not found");

        if (message.PendingActionStatus != "Pending")
            throw new InvalidOperationException("This action has already been resolved.");

        return message;
    }

    private async Task<Message> ProposeCreateServiceTicketAsync(int orgId, int userId, string? argumentsJson)
    {
        using var args = JsonDocument.Parse(argumentsJson ?? "{}");
        var root = args.RootElement;
        var subject = root.TryGetProperty("subject", out var s) ? s.GetString() ?? "" : "";
        var description = root.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
        var categoryName = root.TryGetProperty("category", out var c) ? c.GetString() ?? "" : "";
        var priority = root.TryGetProperty("priority", out var p) ? p.GetString() ?? "Medium" : "Medium";
        if (!ValidPriorities.Contains(priority)) priority = "Medium";

        var employee = await _employeeRepo.GetByUserIdAsync(orgId, userId);
        if (employee is null)
        {
            return new Message
            {
                Role = "assistant",
                Content = "You need to be linked to an Employee record to create tickets via chat — ask an administrator to link your account."
            };
        }

        var categories = await _ticketCategoryService.GetAllAsync(orgId);
        var category = categories.FirstOrDefault(cat => string.Equals(cat.Name, categoryName, StringComparison.OrdinalIgnoreCase));
        if (category is null)
        {
            var validNames = string.Join(", ", categories.Select(cat => cat.Name));
            return new Message
            {
                Role = "assistant",
                Content = $"I couldn't find a ticket category called \"{categoryName}\" — valid categories are: {validNames}."
            };
        }

        var createDto = new CreateServiceTicketDto
        {
            Subject = subject,
            Description = description,
            CategoryId = category.Id,
            RequesterId = employee.Id,
            Priority = priority
        };

        return new Message
        {
            Role = "assistant",
            Content = $"I'll create a Service Ticket:\nSubject: {subject}\nCategory: {category.Name}\nPriority: {priority}\n\nConfirm to proceed.",
            PendingActionType = "CreateServiceTicket",
            PendingActionPayloadJson = JsonSerializer.Serialize(createDto),
            PendingActionStatus = "Pending"
        };
    }

    private static string? ParseWidgetType(string? argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson)) return null;
        using var doc = JsonDocument.Parse(argumentsJson);
        return doc.RootElement.TryGetProperty("widgetType", out var w) ? w.GetString() : null;
    }

    private async Task<Conversation> GetOwnedAsync(int orgId, int userId, int id)
    {
        var conversation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Conversation {id} not found");

        if (conversation.UserId != userId)
            throw new UnauthorizedAccessException("You can only access your own conversations.");

        return conversation;
    }

    private static ConversationSummaryDto ToSummaryDto(Conversation c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        CreatedDate = c.CreatedDate
    };

    private static ConversationDto ToDto(Conversation c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Messages = c.Messages
            .Where(m => m.Role is "user" or "assistant")
            .OrderBy(m => m.CreatedDate)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedDate = m.CreatedDate,
                PendingActionStatus = m.PendingActionStatus
            })
            .ToList()
    };
}
