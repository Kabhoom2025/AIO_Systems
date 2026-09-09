using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Copilot.Commands.GenerateWorkflowGraph;

/// <summary>CurrentGraphJson is optional: when present, the generator is asked to modify that
/// graph in place per the prompt rather than build one from scratch.</summary>
public record GenerateWorkflowGraphCommand(string Prompt, string? CurrentGraphJson = null) : IRequest<Result<GeneratedGraphDto>>;

public record GeneratedGraphDto(string GraphJson);
