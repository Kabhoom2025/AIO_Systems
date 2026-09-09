namespace FlowSphere.Application.Interfaces;

public interface IConnectorCredentialStore
{
    Task<string?> GetAsync(int connectorId, string keyName, int organizationId, CancellationToken cancellationToken);

    Task SetAsync(
        int connectorId, string keyName, string plainValue, int organizationId, int createdByUserId, CancellationToken cancellationToken);
}
