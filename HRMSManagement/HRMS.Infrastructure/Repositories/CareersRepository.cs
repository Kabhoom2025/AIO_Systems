using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class CareersRepository : ICareersRepository
{
    private readonly HrmsDbContext _ctx;

    public CareersRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<Organization?> GetOrgByCodeAsync(string code) =>
        _ctx.Organizations.FirstOrDefaultAsync(o => o.Code == code && o.IsActive);

    public Task<List<JobOpening>> GetOpenOpeningsAsync(int orgId) =>
        _ctx.JobOpenings
            .Include(j => j.Department)
            .Include(j => j.Designation)
            .Where(j => j.OrganizationId == orgId && j.Status == "Open")
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();

    public Task<JobOpening?> GetOpenOpeningAsync(int orgId, int id) =>
        _ctx.JobOpenings
            .Include(j => j.Department)
            .Include(j => j.Designation)
            .FirstOrDefaultAsync(j => j.OrganizationId == orgId && j.Id == id && j.Status == "Open");

    public Task<Candidate?> FindExistingApplicationAsync(int orgId, int openingId, string email) =>
        _ctx.Candidates.FirstOrDefaultAsync(c =>
            c.OrganizationId == orgId && c.JobOpeningId == openingId &&
            c.Email.ToLower() == email.ToLower());

    public Task<Candidate?> GetCandidateByTrackingTokenAsync(string token) =>
        _ctx.Candidates
            .Include(c => c.JobOpening).ThenInclude(j => j.Organization)
            .FirstOrDefaultAsync(c => c.TrackingToken == token);

    public void AddCandidate(Candidate candidate) => _ctx.Candidates.Add(candidate);

    public void AddStageHistory(CandidateStageHistory history) => _ctx.CandidateStageHistories.Add(history);

    public Task<List<CandidateStageHistory>> GetStageHistoryAsync(int candidateId) =>
        _ctx.CandidateStageHistories
            .Where(h => h.CandidateId == candidateId)
            .OrderBy(h => h.CreatedDate)
            .ToListAsync();

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
