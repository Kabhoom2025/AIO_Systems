using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IOtpChallengeRepository
{
    Task<OtpChallenge?> GetActiveByPhoneAsync(string phone);
    void Add(OtpChallenge challenge);
    Task SaveChangesAsync();
}
