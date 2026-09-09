using FlowSphere.Domain.Enums;

namespace FlowSphere.Application.Interfaces;

public class NodeExecutionContext
{
    public required int OrganizationId { get; init; }
    public required string NodeKey { get; init; }
    public required string ConfigJson { get; init; }
    public required string InputJson { get; init; }
    public required IReadOnlyDictionary<string, string?> PriorOutputs { get; init; }
}

public record NodeResult(bool Success, string? OutputJson, string? ErrorMessage, string? NextHandle = null)
{
    public static NodeResult Ok(string? outputJson, string? nextHandle = null) => new(true, outputJson, null, nextHandle);
    public static NodeResult Fail(string errorMessage) => new(false, null, errorMessage);
}

public interface INodeExecutor
{
    WorkflowNodeType SupportedType { get; }
    Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken);
}

public interface INodeExecutorRegistry
{
    INodeExecutor Resolve(WorkflowNodeType nodeType);
}
