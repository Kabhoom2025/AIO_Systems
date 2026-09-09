using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.RequestAppOtp;

public class RequestAppOtpCommandHandler : IRequestHandler<RequestAppOtpCommand, Result>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private readonly Random _random = new();

    private readonly IApplicationDbContext _db;
    private readonly ICurrentEnvironmentContext _currentEnvironment;
    private readonly IPasswordHasher _hasher;
    private readonly IAppNotificationEmailSender _emailSender;
    private readonly IAppOtpSmsSender _smsSender;

    public RequestAppOtpCommandHandler(
        IApplicationDbContext db, ICurrentEnvironmentContext currentEnvironment, IPasswordHasher hasher,
        IAppNotificationEmailSender emailSender, IAppOtpSmsSender smsSender)
    {
        _db = db;
        _currentEnvironment = currentEnvironment;
        _hasher = hasher;
        _emailSender = emailSender;
        _smsSender = smsSender;
    }

    public async Task<Result> Handle(RequestAppOtpCommand request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Stage == _currentEnvironment.Stage && a.IsPublished, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        var code = _random.Next(0, 1_000_000).ToString("D6");
        _db.AppOtpChallenges.Add(new AppOtpChallenge
        {
            AppId = app.Id,
            Channel = request.Channel,
            Recipient = request.Recipient,
            CodeHash = _hasher.Hash(code),
            ExpiresAt = DateTime.UtcNow.Add(CodeLifetime),
        });
        await _db.SaveChangesAsync(cancellationToken);

        if (request.Channel == AppOtpChannel.Mobile)
        {
            var smsResult = await _smsSender.SendAsync(
                app.Workspace.OrganizationId, request.Recipient, $"Your verification code for {app.Name} is {code}. It expires in 10 minutes.", cancellationToken);

            return smsResult.Sent
                ? Result.Success()
                : Result.Failure(Error.Unexpected(smsResult.ErrorMessage ?? "Failed to send the verification SMS."));
        }

        var emailResult = await _emailSender.SendAsync(
            app.Workspace.OrganizationId,
            new[] { request.Recipient },
            Array.Empty<string>(),
            $"Your verification code for {app.Name}",
            $"Your one-time verification code is {code}. It expires in 10 minutes.",
            cancellationToken);

        return emailResult.Sent
            ? Result.Success()
            : Result.Failure(Error.Unexpected(emailResult.ErrorMessage ?? "Failed to send the verification email."));
    }
}
