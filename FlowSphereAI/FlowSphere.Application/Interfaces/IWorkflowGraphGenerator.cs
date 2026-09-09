using FlowSphere.Application.Common;

namespace FlowSphere.Application.Interfaces;

/// <summary>Turns a natural-language description into a WorkflowGraph-shaped JSON string
/// (nodes+edges, matching the same schema the designer canvas saves/loads). Implemented in
/// FlowSphere.Execution using the OpenAi connector - Application only knows the abstraction.</summary>
public interface IWorkflowGraphGenerator
{
    /// <summary>When currentGraphJson is non-null, the generator modifies that graph in place
    /// per the prompt rather than building a new one from scratch.</summary>
    Task<Result<string>> GenerateGraphJsonAsync(
        string prompt, string? currentGraphJson, int organizationId, CancellationToken cancellationToken);
}
