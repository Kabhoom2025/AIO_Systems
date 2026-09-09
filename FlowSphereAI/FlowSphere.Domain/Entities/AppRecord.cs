using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>A real (non-test) submission of an AppDefinition's form, created by Launch. Not
/// ITenantScoped itself - scoped transitively through AppDefinition -> Workspace, same
/// convention as AppDefinition/TableDefinition. IS IEnvironmentScoped directly - a QA submission
/// must never be readable while viewing Live, so it gets a real global query filter rather than
/// relying on a manual join-based check.</summary>
public class AppRecord : BaseEntity, IEnvironmentScoped
{
    public int AppDefinitionId { get; set; }

    /// <summary>The submitted form data, keyed by field key.</summary>
    public string DataJson { get; set; } = "{}";

    /// <summary>Null for Guest/Webhook submissions - there is no authenticated user to attribute
    /// the record to.</summary>
    public int? CreatedByUserId { get; set; }

    public AppRecordSource Source { get; set; } = AppRecordSource.Authenticated;

    public EnvironmentStage Stage { get; set; } = EnvironmentStage.Dev;

    public AppDefinition AppDefinition { get; set; } = null!;

    public static AppRecord Create(
        int appDefinitionId, string dataJson, int? createdByUserId, EnvironmentStage stage = EnvironmentStage.Dev,
        AppRecordSource source = AppRecordSource.Authenticated)
    {
        return new AppRecord
        {
            AppDefinitionId = appDefinitionId,
            DataJson = dataJson,
            CreatedByUserId = createdByUserId,
            Stage = stage,
            Source = source,
        };
    }
}
