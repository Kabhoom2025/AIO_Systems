using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Organizations.Queries.GetOrganizationSettings;

/// <summary>Readable by any authenticated org member (not gated by SettingsManage) - the
/// resulting branding/background applies to everyone's UI, not just admins.</summary>
public record GetOrganizationSettingsQuery : IRequest<Result<OrganizationSettingsDto>>;

public record OrganizationSettingsDto(string Name, string SettingsJson);
