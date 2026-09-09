using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>A row written into a TableDefinition - either directly, or via an integrated
/// AppDefinition's Launch submission (mapped through AppDefinition.FieldMappingJson). Not
/// ITenantScoped itself - scoped transitively through TableDefinition -> Workspace. IS
/// IEnvironmentScoped directly, same rationale as AppRecord.</summary>
public class TableRecord : BaseEntity, IEnvironmentScoped
{
    public int TableDefinitionId { get; set; }

    /// <summary>The row data, keyed by column key.</summary>
    public string DataJson { get; set; } = "{}";

    public int CreatedByUserId { get; set; }

    public EnvironmentStage Stage { get; set; } = EnvironmentStage.Dev;

    public TableDefinition TableDefinition { get; set; } = null!;

    public static TableRecord Create(int tableDefinitionId, string dataJson, int createdByUserId, EnvironmentStage stage = EnvironmentStage.Dev)
    {
        return new TableRecord
        {
            TableDefinitionId = tableDefinitionId,
            DataJson = dataJson,
            CreatedByUserId = createdByUserId,
            Stage = stage,
        };
    }
}
