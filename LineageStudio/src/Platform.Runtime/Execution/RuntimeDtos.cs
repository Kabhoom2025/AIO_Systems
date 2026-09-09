using System.Text.Json;

namespace Platform.Runtime.Execution;

/// <summary>The payload a rendered screen's "Save" (or other action) button posts: which of the
/// application's declared APIs to invoke, and the submitted field values keyed by the API field
/// name each screen component is mapped to (mapping.ApiField), not the component's own name.</summary>
public record RuntimeExecuteRequest(Guid ApiId, Dictionary<string, JsonElement> FormData);

public record RuntimeFieldResult(string ApiField, string ColumnName, string? RawValue, string? TransformedValue);

public record RuntimeExecutionResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage,
    IReadOnlyList<RuntimeFieldResult> Fields,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows);
