using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>The IAiProvider bound in DI — reads the org's AiAssistantSettings.Provider column
/// and delegates every call to the matching concrete provider (AnthropicAiProvider/
/// GroqAiProvider), both registered as themselves via AddHttpClient. AiAssistantService only
/// ever talks to this router through the narrow IAiProvider interface, so it needs no changes
/// to support a second provider.</summary>
public class AiProviderRouter : IAiProvider
{
    private readonly AnthropicAiProvider _anthropic;
    private readonly GroqAiProvider _groq;
    private readonly NovaErpDbContext _ctx;

    public AiProviderRouter(AnthropicAiProvider anthropic, GroqAiProvider groq, NovaErpDbContext ctx)
    {
        _anthropic = anthropic;
        _groq = groq;
        _ctx = ctx;
    }

    public async Task<bool> IsConfiguredAsync(int orgId) =>
        await (await ResolveAsync(orgId)).IsConfiguredAsync(orgId);

    public async Task<AiCompletionResult> CompleteAsync(int orgId, List<AiChatTurn> history, AiToolContext toolContext, CancellationToken ct = default) =>
        await (await ResolveAsync(orgId)).CompleteAsync(orgId, history, toolContext, ct);

    private async Task<IAiProvider> ResolveAsync(int orgId)
    {
        var settings = await _ctx.AiAssistantSettings.FirstOrDefaultAsync(s => s.OrganizationId == orgId);
        return settings?.Provider == "Groq" ? _groq : _anthropic;
    }
}
