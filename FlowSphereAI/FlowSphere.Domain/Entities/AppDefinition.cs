using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>A drag-and-drop-built form (fields + layout) inside a Workspace. Not ITenantScoped
/// itself - scoped transitively through WorkspaceId, mirroring how WorkflowVersion relies on its
/// parent WorkflowDefinition for tenant scoping rather than duplicating OrganizationId. Also not
/// IEnvironmentScoped via the global filter - handlers add an explicit Stage predicate, same
/// pattern as their existing manual Workspace.OrganizationId checks (see PromotedFromAppDefinitionId
/// remarks below for why this is a row-clone rather than a versioned model).</summary>
public class AppDefinition : BaseEntity
{
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Stored lucide-react icon name (see client/features/app-builder/iconCatalog.ts) -
    /// purely cosmetic, shown on app cards and the builder's Name tab.</summary>
    public string? Icon { get; set; }

    /// <summary>Which sandbox stage this row lives in - Dev is where builders always edit;
    /// QA/UAT/Live are produced by promoting a clone of the Dev (or prior-stage) row forward.</summary>
    public EnvironmentStage Stage { get; set; } = EnvironmentStage.Dev;

    /// <summary>Shared across every stage-clone of "the same app" - set once at creation, carried
    /// forward unchanged on every promotion, used to find-or-update the existing row at a target
    /// stage instead of creating duplicates on repeated promotions.</summary>
    public Guid SourceGroupId { get; set; } = Guid.NewGuid();

    /// <summary>Audit trail - the row this one was cloned from when promoted. Null for the
    /// original Dev row.</summary>
    public int? PromotedFromAppDefinitionId { get; set; }

    /// <summary>{ "fields": [ { "key", "type", "label", "required", "placeholder", "options" }, ... ] } -
    /// the drag-and-drop form schema, source of truth for the app builder canvas.</summary>
    public string FormSchemaJson { get; set; } = "{\"fields\":[]}";

    /// <summary>"Rules" for this app - reuses the existing FlowSphere workflow engine instead of a
    /// bespoke rule language. When set, submitting the app (Test or Launch) executes this
    /// workflow's published version with the submitted data as input.</summary>
    public int? LinkedWorkflowDefinitionId { get; set; }

    /// <summary>"Integration" target - when set, a real Launch submission also writes a mapped
    /// TableRecord into this table, translated via FieldMappingJson.</summary>
    public int? LinkedTableId { get; set; }

    /// <summary>{ "formFieldKey": "tableColumnKey", ... } - only meaningful when LinkedTableId is set.</summary>
    public string FieldMappingJson { get; set; } = "{}";

    /// <summary>[ { "id", "name", "enabled", "when": { "fieldKey", "operator", "value" },
    /// "then": { "targetFieldKey", "action" } }, ... ] - field-level conditional logic evaluated
    /// client-side whenever the form's values change (Test and Launch).</summary>
    public string BusinessRulesJson { get; set; } = "[]";

    /// <summary>{ "fieldKey": "Editable" | "Readonly" | "Hidden", ... } - fields absent from the map
    /// default to Editable. Enforced client-side in Test and Launch.</summary>
    public string AccessPermissionsJson { get; set; } = "{}";

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>{ "appOpenLink": "NewRecord"|"AppData", "redirectMode": "DialogPopup"|"PrintRecord"|
    /// "AppData"|"Home"|"NewRecord"|"ViewRecord"|"DownloadRecord", "sectionsDisplay": "List"|"Tabs" } -
    /// absent keys default to the first-listed value client-side. Enforced in the public Launch page.</summary>
    public string SettingsJson { get; set; } = "{}";

    /// <summary>Free-text/markdown help content for this app, shown to end users. Null = no manual
    /// written yet.</summary>
    public string? UserManualMarkdown { get; set; }

    /// <summary>Snapshot of { formSchemaJson, linkedWorkflowDefinitionId, businessRulesJson,
    /// accessPermissionsJson } captured at the moment of the last successful Publish() - powers the
    /// "Review and publish" version comparison. Null until the first publish.</summary>
    public string? PublishedSnapshotJson { get; set; }

    public int CreatedByUserId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public static AppDefinition Create(int workspaceId, string name, string? description, int createdByUserId, string? icon = null)
    {
        return new AppDefinition
        {
            WorkspaceId = workspaceId,
            Name = name,
            Description = description,
            Icon = icon,
            CreatedByUserId = createdByUserId,
        };
    }

    public void UpdateDetails(string name, string? description, string? icon)
    {
        Name = name;
        Description = description;
        Icon = icon;
    }

    public void UpdateFormSchema(string formSchemaJson)
    {
        FormSchemaJson = formSchemaJson;
    }

    public void LinkWorkflow(int? workflowDefinitionId)
    {
        LinkedWorkflowDefinitionId = workflowDefinitionId;
    }

    public void LinkTable(int? tableId, string fieldMappingJson)
    {
        LinkedTableId = tableId;
        FieldMappingJson = tableId is null ? "{}" : fieldMappingJson;
    }

    public void Publish()
    {
        PublishedSnapshotJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            formSchemaJson = FormSchemaJson,
            linkedWorkflowDefinitionId = LinkedWorkflowDefinitionId,
            businessRulesJson = BusinessRulesJson,
            accessPermissionsJson = AccessPermissionsJson,
        });
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
    }

    public void UpdateBusinessRules(string businessRulesJson)
    {
        BusinessRulesJson = businessRulesJson;
    }

    public void UpdateAccessPermissions(string accessPermissionsJson)
    {
        AccessPermissionsJson = accessPermissionsJson;
    }

    public void UpdateSettings(string settingsJson)
    {
        SettingsJson = settingsJson;
    }

    public void UpdateUserManual(string? userManualMarkdown)
    {
        UserManualMarkdown = userManualMarkdown;
    }

    /// <summary>Builds a new row at targetStage carrying this app's current config - used when no
    /// row exists yet for (SourceGroupId, targetStage). IsPublished/PublishedAt carry over too -
    /// Publish is Dev-only (see the guard in PublishAppCommandHandler), so a promoted copy has no
    /// other way to ever become submittable if it didn't inherit this from its source.</summary>
    public AppDefinition CloneForPromotion(EnvironmentStage targetStage)
    {
        return new AppDefinition
        {
            WorkspaceId = WorkspaceId,
            Name = Name,
            Description = Description,
            Icon = Icon,
            FormSchemaJson = FormSchemaJson,
            LinkedWorkflowDefinitionId = LinkedWorkflowDefinitionId,
            LinkedTableId = LinkedTableId,
            FieldMappingJson = FieldMappingJson,
            BusinessRulesJson = BusinessRulesJson,
            AccessPermissionsJson = AccessPermissionsJson,
            CreatedByUserId = CreatedByUserId,
            Stage = targetStage,
            SourceGroupId = SourceGroupId,
            PromotedFromAppDefinitionId = Id,
            IsPublished = IsPublished,
            PublishedAt = PublishedAt,
            SettingsJson = SettingsJson,
            UserManualMarkdown = UserManualMarkdown,
            PublishedSnapshotJson = PublishedSnapshotJson,
        };
    }

    /// <summary>Re-promotion path - an existing target-stage row for this SourceGroupId gets its
    /// config overwritten in place rather than a new row being created.</summary>
    public void ApplyPromotedConfig(AppDefinition source)
    {
        FormSchemaJson = source.FormSchemaJson;
        LinkedWorkflowDefinitionId = source.LinkedWorkflowDefinitionId;
        LinkedTableId = source.LinkedTableId;
        FieldMappingJson = source.FieldMappingJson;
        BusinessRulesJson = source.BusinessRulesJson;
        AccessPermissionsJson = source.AccessPermissionsJson;
        PromotedFromAppDefinitionId = source.Id;
        IsPublished = source.IsPublished;
        PublishedAt = source.PublishedAt;
        SettingsJson = source.SettingsJson;
        UserManualMarkdown = source.UserManualMarkdown;
        PublishedSnapshotJson = source.PublishedSnapshotJson;
    }
}
