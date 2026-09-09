using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.CopyWorkspace;

public record CopyWorkspaceCommand(
    int SourceWorkspaceId,
    string NewWorkspaceName,
    bool CopyApps,
    bool CopyTables,
    int? AdminUserId) : IRequest<Result<int>>;
