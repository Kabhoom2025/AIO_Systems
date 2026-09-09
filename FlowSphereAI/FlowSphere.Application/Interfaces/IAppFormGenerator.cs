using FlowSphere.Application.Common;

namespace FlowSphere.Application.Interfaces;

/// <summary>Turns a natural-language description into a ready-to-save app: a name, an optional
/// description, and a FormSchemaJson string matching the app builder's { "sections": [...] }
/// shape (see formSchema.ts on the client). Implemented in FlowSphere.Execution using the OpenAi
/// connector - Application only knows the abstraction.</summary>
public interface IAppFormGenerator
{
    Task<Result<GeneratedAppFormResult>> GenerateAsync(string prompt, int organizationId, CancellationToken cancellationToken);
}

public record GeneratedAppFormResult(string Name, string? Description, string FormSchemaJson);
