using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Connectors.Commands.SetConnectorCredential;

public class SetConnectorCredentialCommandHandler : IRequestHandler<SetConnectorCredentialCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IConnectorCredentialStore _credentialStore;

    public SetConnectorCredentialCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, IConnectorCredentialStore credentialStore)
    {
        _db = db;
        _currentUser = currentUser;
        _credentialStore = credentialStore;
    }

    public async Task<Result> Handle(SetConnectorCredentialCommand request, CancellationToken cancellationToken)
    {
        var connectorExists = await _db.Connectors.AnyAsync(c => c.Id == request.ConnectorId, cancellationToken);
        if (!connectorExists)
        {
            return Result.Failure(Error.NotFound($"Connector {request.ConnectorId} was not found."));
        }

        await _credentialStore.SetAsync(
            request.ConnectorId, request.KeyName, request.Value, _currentUser.OrganizationId, _currentUser.UserId, cancellationToken);

        return Result.Success();
    }
}
