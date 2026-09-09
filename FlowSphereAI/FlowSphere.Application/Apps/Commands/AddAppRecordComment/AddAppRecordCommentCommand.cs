using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.AddAppRecordComment;

public record AddAppRecordCommentCommand(int AppId, int AppRecordId, string Text) : IRequest<Result<int>>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsSubmit;
}
