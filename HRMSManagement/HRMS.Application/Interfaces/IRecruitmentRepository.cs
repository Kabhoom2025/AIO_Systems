using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IRecruitmentRepository
{
    // Job openings
    Task<List<JobOpening>> GetOpeningsAsync(int orgId, string? status);
    Task<JobOpening?> GetOpeningAsync(int orgId, int id);
    void AddOpening(JobOpening opening);
    void UpdateOpening(JobOpening opening);
    void RemoveOpening(JobOpening opening);

    // Candidates
    Task<List<Candidate>> GetCandidatesByOpeningAsync(int orgId, int openingId);
    Task<Candidate?> GetCandidateAsync(int orgId, int id);
    void AddCandidate(Candidate candidate);
    void UpdateCandidate(Candidate candidate);
    void RemoveCandidate(Candidate candidate);

    // Interviews
    Task<List<Interview>> GetUpcomingInterviewsAsync(int orgId);
    Task<Interview?> GetInterviewAsync(int orgId, int id);
    void AddInterview(Interview interview);
    void UpdateInterview(Interview interview);

    // Pipeline
    Task<Dictionary<string, int>> GetPipelineCountsAsync(int orgId);

    // Stage history
    void AddStageHistory(CandidateStageHistory history);
    Task<List<CandidateStageHistory>> GetStageHistoryAsync(int candidateId);

    Task SaveChangesAsync();
}
