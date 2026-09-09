using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.SubmitApp;

/// <summary>A single submission of an app's form. IsTest=true (from the builder's Test tab) runs
/// the linked workflow so rules can be validated, but writes nothing to storage. IsTest=false
/// (from the public Launch page) requires the app to be published and persists an AppRecord plus,
/// if a table is linked, a mapped TableRecord. SectionId identifies which module (section) is
/// being submitted - when present, that section's own workflow/table link (read out of
/// FormSchemaJson) is used instead of the app-level one, so independent modules in a single app
/// can each run their own approval routing and write to their own table.</summary>
public record SubmitAppCommand(
    int AppId, string DataJson, bool IsTest, string? SectionId = null,
    string? CaptchaToken = null, int? CaptchaAnswer = null,
    string? OtpEmail = null, string? OtpEmailCode = null, string? OtpPhone = null, string? OtpPhoneCode = null)
    : IRequest<Result<SubmitAppResultDto>>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsSubmit;
}

public record SubmitAppResultDto(int? RecordId, string? MappedTableDataJson, Guid? WorkflowExecutionId, bool IsTest, bool UniqueRecordWarning = false, string? UniqueRecordWarningMessage = null);
