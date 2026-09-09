using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetPublicAppInfo;

/// <summary>Anonymous-accessible lookup of a published app's name/description - used only to
/// show "Sign in to continue to {name}" on the login page when someone is redirected there from
/// a launch link they weren't authenticated for yet. Deliberately exposes nothing beyond the
/// name/description, and only for apps that are already published.</summary>
public record GetPublicAppInfoQuery(int Id) : IRequest<Result<PublicAppInfoDto>>;

public record PublicAppInfoDto(string Name, string? Description);
