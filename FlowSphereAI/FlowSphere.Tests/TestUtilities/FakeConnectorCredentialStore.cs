using FlowSphere.Application.Interfaces;

namespace FlowSphere.Tests.TestUtilities;

public class FakeConnectorCredentialStore : IConnectorCredentialStore
{
    private readonly Dictionary<(int connectorId, string keyName, int organizationId), string> _values = new();

    public void Seed(int connectorId, string keyName, int organizationId, string value)
    {
        _values[(connectorId, keyName, organizationId)] = value;
    }

    public Task<string?> GetAsync(int connectorId, string keyName, int organizationId, CancellationToken cancellationToken)
    {
        _values.TryGetValue((connectorId, keyName, organizationId), out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync(int connectorId, string keyName, string plainValue, int organizationId, int createdByUserId, CancellationToken cancellationToken)
    {
        _values[(connectorId, keyName, organizationId)] = plainValue;
        return Task.CompletedTask;
    }
}
