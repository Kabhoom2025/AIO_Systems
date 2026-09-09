using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;

namespace FlowSphere.Application.Workflows.Commands.CreateWorkflow;

public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CreateWorkflowCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = WorkflowDefinition.Create(
            _currentUser.OrganizationId, request.Name, request.Description, _currentUser.UserId);

        _db.WorkflowDefinitions.Add(workflow);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(workflow.Id);
    }
}
