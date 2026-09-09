namespace FlowSphere.Execution.Connectors;

public record ConnectorInvocation(string ConfigJson, string InputJson);
public record ConnectorResult(bool Success, string? OutputJson, string? ErrorMessage)
{
    public static ConnectorResult Ok(string? outputJson) => new(true, outputJson, null);
    public static ConnectorResult Fail(string errorMessage) => new(false, null, errorMessage);
}

public interface IConnector
{
    string Type { get; }
    Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken);
}
