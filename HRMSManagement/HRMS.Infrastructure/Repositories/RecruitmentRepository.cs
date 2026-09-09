using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class RecruitmentRepository : IRecruitmentRepository
{
    private readonly HrmsDbContext _ctx;

    public RecruitmentRepository(HrmsDbContext ctx) => _ctx = ctx;

    // Job openings
    public Task<List<JobOpening>> GetOpeningsAsync(int orgId, string? status)
    {
        var query = _ctx.JobOpenings
            .Include(j => j.Department)
            .Include(j => j.Designation)
            .Include(j => j.Candidates)
            .Where(j => j.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(j => j.Status == status);

        return query.OrderByDescending(j => j.PostedDate).ToListAsync();
    }

    public Task<JobOpening?> GetOpeningAsync(int orgId, int id) =>
        _ctx.JobOpenings
            .Include(j => j.Department)
            .Include(j => j.Designation)
            .Include(j => j.Candidates)
            .FirstOrDefaultAsync(j => j.OrganizationId == orgId && j.Id == id);

    public void AddOpening(JobOpening opening)    => _ctx.JobOpenings.Add(opening);
    public void UpdateOpening(JobOpening opening) => _ctx.JobOpenings.Update(opening);
    public void RemoveOpening(JobOpening opening) => _ctx.JobOpenings.Remove(opening);

    // Candidates
    public Task<List<Candidate>> GetCandidatesByOpeningAsync(int orgId, int openingId) =>
        _ctx.Candidates
            .Include(c => c.JobOpening)
            .Where(c => c.OrganizationId == orgId && c.JobOpeningId == openingId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

    public Task<Candidate?> GetCandidateAsync(int orgId, int id) =>
        _ctx.Candidates
            .Include(c => c.JobOpening)
            .Include(c => c.Interviews)
            .FirstOrDefaultAsync(c => c.OrganizationId == orgId && c.Id == id);

    public void AddCandidate(Candidate candidate)    => _ctx.Candidates.Add(candidate);
    public void UpdateCandidate(Candidate candidate) => _ctx.Candidates.Update(candidate);
    public void RemoveCandidate(Candidate candidate) => _ctx.Candidates.Remove(candidate);

    // Interviews
    public Task<List<Interview>> GetUpcomingInterviewsAsync(int orgId) =>
        _ctx.Interviews
            .Include(i => i.Candidate).ThenInclude(c => c.JobOpening)
            .Where(i => i.Candidate.OrganizationId == orgId
                        && i.Status == "Scheduled"
                        && i.ScheduledAt >= DateTime.UtcNow)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync();

    public Task<Interview?> GetInterviewAsync(int orgId, int id) =>
        _ctx.Interviews
            .Include(i => i.Candidate).ThenInclude(c => c.JobOpening)
            .FirstOrDefaultAsync(i => i.Candidate.OrganizationId == orgId && i.Id == id);

    public void AddInterview(Interview interview)    => _ctx.Interviews.Add(interview);
    public void UpdateInterview(Interview interview) => _ctx.Interviews.Update(interview);

    // Pipeline
    public async Task<Dictionary<string, int>> GetPipelineCountsAsync(int orgId)
    {
        var grouped = await _ctx.Candidates
            .Where(c => c.OrganizationId == orgId)
            .GroupBy(c => c.Stage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync();

        return grouped.ToDictionary(g => g.Stage, g => g.Count);
    }

    // Stage history
    public void AddStageHistory(CandidateStageHistory history) => _ctx.CandidateStageHistories.Add(history);

    public Task<List<CandidateStageHistory>> GetStageHistoryAsync(int candidateId) =>
        _ctx.CandidateStageHistories
            .Where(h => h.CandidateId == candidateId)
            .OrderBy(h => h.CreatedDate)
            .ToListAsync();

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
