using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class OtpChallengeRepository : IOtpChallengeRepository
{
    private readonly PharmacyDbContext _ctx;

    public OtpChallengeRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<OtpChallenge?> GetActiveByPhoneAsync(string phone) =>
        _ctx.OtpChallenges
            .Where(o => o.Phone == phone && !o.Consumed && o.ExpiresAt >= DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedDate)
            .FirstOrDefaultAsync();

    public void Add(OtpChallenge challenge) => _ctx.OtpChallenges.Add(challenge);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
