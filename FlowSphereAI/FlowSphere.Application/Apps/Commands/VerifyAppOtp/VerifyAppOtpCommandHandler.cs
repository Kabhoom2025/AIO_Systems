using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.VerifyAppOtp;

public class VerifyAppOtpCommandHandler : IRequestHandler<VerifyAppOtpCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;

    public VerifyAppOtpCommandHandler(IApplicationDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Result> Handle(VerifyAppOtpCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var candidates = await _db.AppOtpChallenges
            .Where(c => c.AppId == request.AppId && c.Channel == request.Channel && c.Recipient == request.Recipient && !c.Consumed && c.ExpiresAt > now)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);

        var match = candidates.FirstOrDefault(c => _hasher.Verify(request.Code, c.CodeHash));
        if (match is null)
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["code"] = new[] { "Invalid or expired verification code." },
            }));
        }

        match.Consumed = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
