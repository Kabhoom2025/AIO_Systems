using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;

namespace FlowSphere.Application.Copilot.Commands.GenerateWorkflowGraph;

public class GenerateWorkflowGraphCommandHandler : IRequestHandler<GenerateWorkflowGraphCommand, Result<GeneratedGraphDto>>
{
    private readonly IWorkflowGraphGenerator _generator;
    private readonly ICurrentUserContext _currentUser;

    public GenerateWorkflowGraphCommandHandler(IWorkflowGraphGenerator generator, ICurrentUserContext currentUser)
    {
        _generator = generator;
        _currentUser = currentUser;
    }

    public async Task<Result<GeneratedGraphDto>> Handle(GenerateWorkflowGraphCommand request, CancellationToken cancellationToken)
    {
        var result = await _generator.GenerateGraphJsonAsync(
            request.Prompt, request.CurrentGraphJson, _currentUser.OrganizationId, cancellationToken);

        return result.IsSuccess
            ? Result<GeneratedGraphDto>.Success(new GeneratedGraphDto(result.Value!))
            : Result<GeneratedGraphDto>.Failure(result.Error!);
    }
}
