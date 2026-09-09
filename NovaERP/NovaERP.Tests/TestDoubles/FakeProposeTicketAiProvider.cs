using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Tests.TestDoubles;

/// <summary>A fake IAiProvider that always "wants" to call propose_create_service_ticket,
/// regardless of message content — used to exercise AiAssistantService's real propose code path
/// end-to-end through SendMessage, which none of the DB-seeded confirm/cancel tests do (those
/// bypass the propose step entirely by inserting the Pending message directly).</summary>
public class FakeProposeTicketAiProvider : IAiProvider
{
    public Task<bool> IsConfiguredAsync(int orgId) => Task.FromResult(true);

    public Task<AiCompletionResult> CompleteAsync(int orgId, List<AiChatTurn> history, AiToolContext toolContext, CancellationToken ct = default)
    {
        if (!toolContext.CanCreateServiceTickets)
            return Task.FromResult(new AiCompletionResult { FinalText = "I can't create tickets for you." });

        return Task.FromResult(new AiCompletionResult
        {
            ToolCallName = "propose_create_service_ticket",
            ToolCallArgumentsJson = """{"subject":"Printer not working","description":"The 3rd floor printer is jammed.","category":"Hardware","priority":"Medium"}""",
            ToolCallId = "fake-tool-call-1"
        });
    }
}
