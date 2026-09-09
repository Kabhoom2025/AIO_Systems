using System.Text.Json;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>Config shape: { "milliseconds": number }. Bounded to a sane Phase 1 max so a
/// misconfigured node can't hang a dispatcher worker indefinitely.</summary>
public class DelayNodeExecutor : INodeExecutor
{
    private const int MaxDelayMilliseconds = 60_000;

    public WorkflowNodeType SupportedType => WorkflowNodeType.Delay;

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(context.ConfigJson);
        var requested = config.RootElement.TryGetProperty("milliseconds", out var msEl) && msEl.TryGetInt32(out var ms)
            ? ms
            : 1000;

        var bounded = Math.Clamp(requested, 0, MaxDelayMilliseconds);
        await Task.Delay(bounded, cancellationToken);

        return NodeResult.Ok(context.InputJson);
    }
}
