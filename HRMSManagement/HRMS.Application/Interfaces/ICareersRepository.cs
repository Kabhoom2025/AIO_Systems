using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface ICareersRepository
{
    Task<Organization?> GetOrgByCodeAsync(string code);
    Task<List<JobOpening>> GetOpenOpeningsAsync(int orgId);
    Task<JobOpening?> GetOpenOpeningAsync(int orgId, int id);
    Task<Candidate?> FindExistingApplicationAsync(int orgId, int openingId, string email);
    Task<Candidate?> GetCandidateByTrackingTokenAsync(string token);
    void AddCandidate(Candidate candidate);
    void AddStageHistory(CandidateStageHistory history);
    Task<List<CandidateStageHistory>> GetStageHistoryAsync(int candidateId);
    Task SaveChangesAsync();
}
