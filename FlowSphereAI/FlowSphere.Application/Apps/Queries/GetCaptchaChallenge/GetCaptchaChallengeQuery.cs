using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetCaptchaChallenge;

public record GetCaptchaChallengeQuery : IRequest<Result<CaptchaChallenge>>;
