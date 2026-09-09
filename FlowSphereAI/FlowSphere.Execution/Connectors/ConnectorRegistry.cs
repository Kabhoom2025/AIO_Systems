namespace FlowSphere.Execution.Connectors;

public class ConnectorRegistry
{
    private readonly Dictionary<string, IConnector> _connectorsByType;

    public ConnectorRegistry(IEnumerable<IConnector> connectors)
    {
        _connectorsByType = connectors.ToDictionary(c => c.Type, StringComparer.OrdinalIgnoreCase);
    }

    public IConnector Resolve(string type)
    {
        if (!_connectorsByType.TryGetValue(type, out var connector))
        {
            throw new InvalidOperationException($"No connector registered for type '{type}'.");
        }

        return connector;
    }
}
