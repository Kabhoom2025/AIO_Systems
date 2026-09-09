using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Organizations.Commands.SaveOrganizationSettings;

/// <summary>Not IWorkspaceScopedRequest/IAppScopedRequest - this is org-wide, gated purely by the
/// SettingsManage policy on the controller action (same pattern BillingManage-gated actions use).</summary>
public record SaveOrganizationSettingsCommand(string SettingsJson) : IRequest<Result>;
