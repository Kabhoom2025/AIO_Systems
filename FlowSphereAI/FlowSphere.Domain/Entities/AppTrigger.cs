using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>Cross-app automation: when a record is submitted in SourceAppId (the only firing
/// point wired up today - see AppTriggerDispatcher), map its fields into DestinationAppId's shape
/// and create a record there, optionally also starting the destination app's linked workflow
/// (ActionType == CreateTask). Both AppIds are AppDefinition.Id values (stage-specific rows, same
/// convention as every other app reference in this module) - a trigger only fires between two
/// rows in the same stage.</summary>
public class AppTrigger : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SourceAppId { get; set; }
    public int DestinationAppId { get; set; }
    public AppTriggerActionType ActionType { get; set; } = AppTriggerActionType.SubmitAddRecord;

    /// <summary>{ "sourceFieldKey": "destinationFieldKey", ... } - same shallow shape as
    /// AppDefinition.FieldMappingJson. A source User Reference field maps through here like any
    /// other field - if the destination app has a matching field, the picked user(s) carry over
    /// as ordinary submitted data. This does NOT force-route the destination workflow's approval
    /// step to that user - the destination workflow's own role-based step assignment still
    /// decides who the resulting task actually goes to.</summary>
    public string FieldMappingJson { get; set; } = "{}";

    public bool IsEnabled { get; set; } = true;
}
