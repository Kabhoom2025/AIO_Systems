using System.Security.Cryptography;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowSphere.Application.Auth.Commands.ResendVerificationEmail;

public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ClientSettings _clientSettings;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

    public ResendVerificationEmailCommandHandler(
        IApplicationDbContext db,
        ICurrentUserContext currentUser,
        IOptions<ClientSettings> clientSettings,
        ILogger<ResendVerificationEmailCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _clientSettings = clientSettings.Value;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User not found."));
        }

        if (user.IsEmailVerified)
        {
            return Result.Success();
        }

        var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        await _db.SaveChangesAsync(cancellationToken);

        var verificationLink = $"{_clientSettings.BaseUrl}/verify-email?token={verificationToken}";
        _logger.LogInformation("Email verification link for {Email}: {Link}", user.Email, verificationLink);

        return Result.Success();
    }
}
