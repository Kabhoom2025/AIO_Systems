namespace Platform.Runtime.Execution;

public interface IRuntimeExecutionService
{
    Task<RuntimeExecutionResult> ExecuteAsync(Guid applicationId, RuntimeExecuteRequest request, CancellationToken ct = default);

    /// <summary>Reads existing rows of one of the application's own tables (never platform.*
    /// metadata) - used to populate a foreign-key picker in the public runtime form, so an end
    /// user picks a real related row instead of typing a raw UUID by hand.</summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ListTableRowsAsync(
        Guid applicationId, Guid tableId, CancellationToken ct = default);

    /// <summary>Most-recently-written rows of one of the application's own tables first - backs
    /// the Data Designer "latest data" viewer, so a builder can see what has actually flowed into
    /// the database without leaving the app.</summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ListLatestRowsAsync(
        Guid applicationId, Guid tableId, int limit, CancellationToken ct = default);
}
