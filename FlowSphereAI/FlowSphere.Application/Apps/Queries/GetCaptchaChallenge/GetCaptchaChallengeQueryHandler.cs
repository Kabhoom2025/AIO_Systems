using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetCaptchaChallenge;

public class GetCaptchaChallengeQueryHandler : IRequestHandler<GetCaptchaChallengeQuery, Result<CaptchaChallenge>>
{
    private readonly ICaptchaService _captcha;

    public GetCaptchaChallengeQueryHandler(ICaptchaService captcha)
    {
        _captcha = captcha;
    }

    public Task<Result<CaptchaChallenge>> Handle(GetCaptchaChallengeQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<CaptchaChallenge>.Success(_captcha.GenerateChallenge()));
}
