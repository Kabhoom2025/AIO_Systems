using FlowSphere.Application.Apps.Commands.AddAppRecordComment;
using FlowSphere.Application.Apps.Commands.CreateApp;
using FlowSphere.Application.Apps.Commands.DeleteApp;
using FlowSphere.Application.Apps.Commands.DeleteAppNotificationRule;
using FlowSphere.Application.Apps.Commands.DeleteAppTrigger;
using FlowSphere.Application.Apps.Commands.DiscardApp;
using FlowSphere.Application.Apps.Commands.GenerateAppShareToken;
using FlowSphere.Application.Apps.Commands.IngestAppWebhookRecord;
using FlowSphere.Application.Apps.Commands.LinkAppTable;
using FlowSphere.Application.Apps.Commands.LinkAppWorkflow;
using FlowSphere.Application.Apps.Commands.MoveAppToWorkspace;
using FlowSphere.Application.Apps.Commands.PromoteApp;
using FlowSphere.Application.Apps.Commands.PublishApp;
using FlowSphere.Application.Apps.Commands.RequestAppOtp;
using FlowSphere.Application.Apps.Commands.RollbackApp;
using FlowSphere.Application.Apps.Commands.SaveAppNotificationRule;
using FlowSphere.Application.Apps.Commands.SaveAppTrigger;
using FlowSphere.Application.Apps.Commands.SubmitGuestAppRecord;
using FlowSphere.Application.Apps.Commands.VerifyAppOtp;
using FlowSphere.Application.Apps.Queries.GetAppNotificationRules;
using FlowSphere.Application.Apps.Queries.GetAppRecordComments;
using FlowSphere.Application.Apps.Queries.GetAppTriggers;
using FlowSphere.Application.Apps.Queries.GetCaptchaChallenge;
using FlowSphere.Application.Apps.Queries.GetDeploymentLog;
using FlowSphere.Application.Apps.Queries.GetDeploymentPipeline;
using FlowSphere.Application.Apps.Commands.SaveAppAccessPermissions;
using FlowSphere.Application.Apps.Commands.SaveAppBusinessRules;
using FlowSphere.Application.Apps.Commands.SaveAppForm;
using FlowSphere.Application.Apps.Commands.SaveAppSettings;
using FlowSphere.Application.Apps.Commands.SaveAppUserManual;
using FlowSphere.Application.Apps.Commands.SubmitApp;
using FlowSphere.Application.Apps.Commands.UpdateAppDetails;
using FlowSphere.Application.Apps.Queries.GetAppById;
using FlowSphere.Application.Apps.Queries.GetAppPublishHistory;
using FlowSphere.Application.Apps.Queries.GetAppRecordsList;
using FlowSphere.Application.Apps.Queries.GetAppsList;
using FlowSphere.Application.Apps.Queries.GetPublicAppInfo;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api")]
public class AppsController : ApiControllerBase
{
    [HttpGet("workspaces/{workspaceId:int}/apps")]
    [Authorize(Policy = PermissionCatalog.AppsRead)]
    public async Task<IActionResult> GetList(int workspaceId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppsListQuery(workspaceId), cancellationToken);
        return FromResult(result);
    }

    public record CreateAppRequest(string Name, string? Description, string? Icon = null);

    // apps.write is enforced by WorkspacePermissionBehavior (a global-role grant OR a
    // workspace-scoped WorkspaceRoleAssignment both satisfy it) - see the 16 IWorkspaceScopedRequest/
    // IAppScopedRequest/ITableScopedRequest commands. The bare class-level [Authorize] above still
    // requires the caller to be authenticated and in this org.
    [HttpPost("workspaces/{workspaceId:int}/apps")]
    public async Task<IActionResult> Create(int workspaceId, CreateAppRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CreateAppCommand(workspaceId, request.Name, request.Description, request.Icon), cancellationToken);
        return FromResult(result, id => CreatedAtAction(nameof(GetById), new { id }, new { id }));
    }

    public record UpdateDetailsRequest(string Name, string? Description, string? Icon);

    [HttpPut("apps/{id:int}/details")]
    public async Task<IActionResult> UpdateDetails(int id, UpdateDetailsRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateAppDetailsCommand(id, request.Name, request.Description, request.Icon), cancellationToken);
        return FromResult(result);
    }

    public record MoveRequest(int TargetWorkspaceId);

    [HttpPost("apps/{id:int}/move")]
    public async Task<IActionResult> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new MoveAppToWorkspaceCommand(id, request.TargetWorkspaceId), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}")]
    [Authorize(Policy = PermissionCatalog.AppsRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppByIdQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}/public-info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicInfo(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPublicAppInfoQuery(id), cancellationToken);
        return FromResult(result);
    }

    public record SaveFormRequest(string FormSchemaJson);

    [HttpPut("apps/{id:int}/form")]
    public async Task<IActionResult> SaveForm(int id, SaveFormRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppFormCommand(id, request.FormSchemaJson), cancellationToken);
        return FromResult(result);
    }

    public record LinkWorkflowRequest(int? WorkflowDefinitionId);

    [HttpPut("apps/{id:int}/workflow-link")]
    public async Task<IActionResult> LinkWorkflow(int id, LinkWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LinkAppWorkflowCommand(id, request.WorkflowDefinitionId), cancellationToken);
        return FromResult(result);
    }

    public record LinkTableRequest(int? TableId, string FieldMappingJson);

    [HttpPut("apps/{id:int}/table-link")]
    public async Task<IActionResult> LinkTable(int id, LinkTableRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LinkAppTableCommand(id, request.TableId, request.FieldMappingJson), cancellationToken);
        return FromResult(result);
    }

    public record SaveBusinessRulesRequest(string BusinessRulesJson);

    [HttpPut("apps/{id:int}/business-rules")]
    public async Task<IActionResult> SaveBusinessRules(int id, SaveBusinessRulesRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppBusinessRulesCommand(id, request.BusinessRulesJson), cancellationToken);
        return FromResult(result);
    }

    public record SaveAccessPermissionsRequest(string AccessPermissionsJson);

    [HttpPut("apps/{id:int}/access-permissions")]
    public async Task<IActionResult> SaveAccessPermissions(int id, SaveAccessPermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppAccessPermissionsCommand(id, request.AccessPermissionsJson), cancellationToken);
        return FromResult(result);
    }

    public record PublishRequest(string? Comment = null);

    [HttpPost("apps/{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, [FromBody] PublishRequest? request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new PublishAppCommand(id, request?.Comment), cancellationToken);
        return FromResult(result);
    }

    public record SaveSettingsRequest(string SettingsJson);

    [HttpPut("apps/{id:int}/settings")]
    public async Task<IActionResult> SaveSettings(int id, SaveSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppSettingsCommand(id, request.SettingsJson), cancellationToken);
        return FromResult(result);
    }

    public record SaveUserManualRequest(string? UserManualMarkdown);

    [HttpPut("apps/{id:int}/user-manual")]
    public async Task<IActionResult> SaveUserManual(int id, SaveUserManualRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppUserManualCommand(id, request.UserManualMarkdown), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}/publish-history")]
    [Authorize(Policy = PermissionCatalog.AppsRead)]
    public async Task<IActionResult> GetPublishHistory(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppPublishHistoryQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpPost("apps/{id:int}/promote")]
    public async Task<IActionResult> Promote(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new PromoteAppCommand(id), cancellationToken);
        return FromResult(result);
    }

    [HttpPost("apps/{id:int}/rollback")]
    public async Task<IActionResult> Rollback(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RollbackAppCommand(id), cancellationToken);
        return FromResult(result);
    }

    [HttpPost("apps/{id:int}/discard")]
    public async Task<IActionResult> Discard(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DiscardAppCommand(id), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/deployment-pipeline")]
    [Authorize(Policy = PermissionCatalog.AppsRead)]
    public async Task<IActionResult> GetDeploymentPipeline(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDeploymentPipelineQuery(), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/deployment-log")]
    [Authorize(Policy = PermissionCatalog.AppsRead)]
    public async Task<IActionResult> GetDeploymentLog([FromQuery] EnvironmentStage? stage, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDeploymentLogQuery(stage), cancellationToken);
        return FromResult(result);
    }

    public record SubmitAppRequest(
        string DataJson, bool IsTest, string? SectionId = null,
        string? CaptchaToken = null, int? CaptchaAnswer = null,
        string? OtpEmail = null, string? OtpEmailCode = null, string? OtpPhone = null, string? OtpPhoneCode = null);

    [HttpPost("apps/{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, SubmitAppRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SubmitAppCommand(
            id, request.DataJson, request.IsTest, request.SectionId,
            request.CaptchaToken, request.CaptchaAnswer,
            request.OtpEmail, request.OtpEmailCode, request.OtpPhone, request.OtpPhoneCode), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}/records")]
    public async Task<IActionResult> GetRecords(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppRecordsListQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("apps/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteAppCommand(id), cancellationToken);
        return FromResult(result);
    }

    public record GenerateShareTokenRequest(string Kind);

    [HttpPost("apps/{id:int}/share-token")]
    public async Task<IActionResult> GenerateShareToken(int id, GenerateShareTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GenerateAppShareTokenCommand(id, request.Kind), cancellationToken);
        return FromResult(result);
    }

    public record GuestSubmitRequest(
        string GuestToken, string DataJson, string? SectionId = null,
        string? CaptchaToken = null, int? CaptchaAnswer = null,
        string? OtpEmail = null, string? OtpEmailCode = null, string? OtpPhone = null, string? OtpPhoneCode = null);

    [HttpPost("apps/{id:int}/guest-submit")]
    [AllowAnonymous]
    public async Task<IActionResult> GuestSubmit(int id, GuestSubmitRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SubmitGuestAppRecordCommand(
            id, request.GuestToken, request.DataJson, request.SectionId, request.CaptchaToken, request.CaptchaAnswer,
            request.OtpEmail, request.OtpEmailCode, request.OtpPhone, request.OtpPhoneCode), cancellationToken);
        return FromResult(result);
    }

    public record WebhookIngestRequest(string ApiKey, string DataJson);

    [HttpPost("apps/{id:int}/webhook-ingest")]
    [AllowAnonymous]
    public async Task<IActionResult> WebhookIngest(int id, WebhookIngestRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new IngestAppWebhookRecordCommand(id, request.ApiKey, request.DataJson), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/captcha-challenge")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCaptchaChallenge(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCaptchaChallengeQuery(), cancellationToken);
        return FromResult(result);
    }

    public record RequestOtpRequest(AppOtpChannel Channel, string Recipient);

    [HttpPost("apps/{id:int}/otp/request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestOtp(int id, RequestOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RequestAppOtpCommand(id, request.Channel, request.Recipient), cancellationToken);
        return FromResult(result);
    }

    public record VerifyOtpRequest(AppOtpChannel Channel, string Recipient, string Code);

    [HttpPost("apps/{id:int}/otp/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp(int id, VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new VerifyAppOtpCommand(id, request.Channel, request.Recipient, request.Code), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}/notification-rules")]
    public async Task<IActionResult> GetNotificationRules(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppNotificationRulesQuery(id), cancellationToken);
        return FromResult(result);
    }

    public record SaveNotificationRuleRequest(
        int? Id, string Name, AppNotificationEvent Event, AppNotificationLevel Level, string? StepKey, string? ActionKey,
        AppNotificationChannel Channel, string? SenderEmail, AppNotificationRecipientType RecipientType,
        List<int> RecipientRoleIds, List<int> RecipientUserIds, string? RecipientDataFieldKey, List<int> CcUserIds,
        string Subject, string Body, string ConditionsJson, bool SendAttachments, bool SendReports, bool IsEnabled);

    [HttpPut("apps/{id:int}/notification-rules")]
    public async Task<IActionResult> SaveNotificationRule(int id, SaveNotificationRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppNotificationRuleCommand(
            request.Id, id, request.Name, request.Event, request.Level, request.StepKey, request.ActionKey,
            request.Channel, request.SenderEmail, request.RecipientType,
            request.RecipientRoleIds, request.RecipientUserIds, request.RecipientDataFieldKey, request.CcUserIds,
            request.Subject, request.Body, request.ConditionsJson, request.SendAttachments, request.SendReports, request.IsEnabled), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("apps/{id:int}/notification-rules/{ruleId:int}")]
    public async Task<IActionResult> DeleteNotificationRule(int id, int ruleId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteAppNotificationRuleCommand(id, ruleId), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}/records/{recordId:int}/comments")]
    public async Task<IActionResult> GetRecordComments(int id, int recordId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppRecordCommentsQuery(id, recordId), cancellationToken);
        return FromResult(result);
    }

    public record AddCommentRequest(string Text);

    [HttpPost("apps/{id:int}/records/{recordId:int}/comments")]
    public async Task<IActionResult> AddRecordComment(int id, int recordId, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AddAppRecordCommentCommand(id, recordId, request.Text), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("apps/{id:int}/triggers")]
    public async Task<IActionResult> GetTriggers(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAppTriggersQuery(id), cancellationToken);
        return FromResult(result);
    }

    public record SaveTriggerRequest(int? Id, string Name, int DestinationAppId, AppTriggerActionType ActionType, string FieldMappingJson, bool IsEnabled);

    [HttpPut("apps/{id:int}/triggers")]
    public async Task<IActionResult> SaveTrigger(int id, SaveTriggerRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveAppTriggerCommand(
            request.Id, id, request.Name, request.DestinationAppId, request.ActionType, request.FieldMappingJson, request.IsEnabled), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("apps/{id:int}/triggers/{triggerId:int}")]
    public async Task<IActionResult> DeleteTrigger(int id, int triggerId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteAppTriggerCommand(id, triggerId), cancellationToken);
        return FromResult(result);
    }
}
