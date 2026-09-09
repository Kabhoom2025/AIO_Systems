using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Copilot.Commands.GenerateApp;

public record GenerateAppCommand(string Prompt) : IRequest<Result<GeneratedAppDto>>;

public record GeneratedAppDto(string Name, string? Description, string FormSchemaJson);
