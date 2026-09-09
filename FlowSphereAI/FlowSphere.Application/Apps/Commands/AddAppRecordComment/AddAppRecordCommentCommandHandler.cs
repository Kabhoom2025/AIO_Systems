using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.AddAppRecordComment;

public class AddAppRecordCommentCommandHandler : IRequestHandler<AddAppRecordCommentCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public AddAppRecordCommentCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(AddAppRecordCommentCommand request, CancellationToken cancellationToken)
    {
        var recordExists = await _db.AppRecords
            .AnyAsync(r => r.Id == request.AppRecordId && r.AppDefinitionId == request.AppId, cancellationToken);
        if (!recordExists)
        {
            return Result<int>.Failure(Error.NotFound($"Record {request.AppRecordId} was not found."));
        }

        var userName = await _db.Users.Where(u => u.Id == _currentUser.UserId).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken) ?? "Unknown user";

        var comment = new AppRecordComment
        {
            OrganizationId = _currentUser.OrganizationId,
            AppRecordId = request.AppRecordId,
            UserId = _currentUser.UserId,
            UserName = userName,
            Text = request.Text,
        };
        _db.AppRecordComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(comment.Id);
    }
}
