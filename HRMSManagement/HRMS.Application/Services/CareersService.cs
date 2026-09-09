using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class CareersService : ICareersService
{
    private readonly ICareersRepository _repo;

    public CareersService(ICareersRepository repo) => _repo = repo;

    public async Task<PublicOrgDto> GetOrgAsync(string orgCode)
    {
        var org = await GetOwnedOrgAsync(orgCode);
        return new PublicOrgDto { Name = org.Name, Code = org.Code };
    }

    public async Task<List<PublicJobOpeningDto>> GetOpeningsAsync(string orgCode)
    {
        var org = await GetOwnedOrgAsync(orgCode);
        var openings = await _repo.GetOpenOpeningsAsync(org.Id);
        return openings.Select(MapToDto).ToList();
    }

    public async Task<PublicJobOpeningDetailDto> GetOpeningAsync(string orgCode, int id)
    {
        var org = await GetOwnedOrgAsync(orgCode);
        var opening = await _repo.GetOpenOpeningAsync(org.Id, id)
            ?? throw new KeyNotFoundException("This job opening is not available.");

        return new PublicJobOpeningDetailDto
        {
            Id                 = opening.Id,
            Title              = opening.Title,
            DepartmentName     = opening.Department?.Name ?? string.Empty,
            DesignationTitle   = opening.Designation?.Title ?? string.Empty,
            Location           = opening.Location,
            EmploymentType     = opening.EmploymentType,
            MinExperienceYears = opening.MinExperienceYears,
            MaxExperienceYears = opening.MaxExperienceYears,
            SalaryRangeFrom    = opening.SalaryRangeFrom,
            SalaryRangeTo      = opening.SalaryRangeTo,
            Vacancies          = opening.Vacancies,
            PostedDate         = opening.PostedDate,
            Description        = opening.Description
        };
    }

    public async Task<ApplyResultDto> ApplyAsync(string orgCode, int openingId, ApplyToJobDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            throw new InvalidOperationException("Name and email are required.");

        var org = await GetOwnedOrgAsync(orgCode);
        var opening = await _repo.GetOpenOpeningAsync(org.Id, openingId)
            ?? throw new KeyNotFoundException("This job opening is not available.");

        var existing = await _repo.FindExistingApplicationAsync(org.Id, opening.Id, dto.Email);
        if (existing != null)
        {
            return new ApplyResultDto
            {
                CandidateId   = existing.Id,
                TrackingToken = existing.TrackingToken,
                Message       = "You've already applied for this position. Here's your existing tracking link."
            };
        }

        var candidate = new Candidate
        {
            OrganizationId       = org.Id,
            JobOpeningId         = opening.Id,
            Name                 = dto.Name,
            Email                = dto.Email,
            Phone                = dto.Phone,
            ResumeUrl            = dto.ResumeUrl,
            CurrentCompany       = dto.CurrentCompany,
            TotalExperienceYears = dto.TotalExperienceYears,
            ExpectedSalary       = dto.ExpectedSalary,
            Source               = string.IsNullOrWhiteSpace(dto.Source) ? "Portal" : dto.Source,
            Notes                = dto.CoverNote,
            Stage                = "Applied",
            TrackingToken        = Guid.NewGuid().ToString("N")
        };
        _repo.AddCandidate(candidate);
        await _repo.SaveChangesAsync();

        _repo.AddStageHistory(new CandidateStageHistory
        {
            CandidateId   = candidate.Id,
            Stage         = "Applied",
            Notes         = "Application submitted via the careers portal.",
            ChangedByName = "Candidate"
        });
        await _repo.SaveChangesAsync();

        return new ApplyResultDto
        {
            CandidateId   = candidate.Id,
            TrackingToken = candidate.TrackingToken,
            Message       = "Application submitted successfully."
        };
    }

    public async Task<TrackingStatusDto> TrackAsync(string token)
    {
        var candidate = await _repo.GetCandidateByTrackingTokenAsync(token)
            ?? throw new KeyNotFoundException("Invalid tracking link.");

        var history = await _repo.GetStageHistoryAsync(candidate.Id);
        var isRejected = candidate.Stage == "Rejected";
        var isHired = candidate.Stage == "Hired";
        var progressPercent = CandidateStageProgress.GetProgressPercent(candidate.Stage, history.Select(h => h.Stage));

        var reachedIndex = isRejected
            ? history.Select(h => Array.IndexOf(CandidateStageProgress.Stages, h.Stage)).Where(i => i >= 0).DefaultIfEmpty(-1).Max()
            : Array.IndexOf(CandidateStageProgress.Stages, candidate.Stage);

        var stages = CandidateStageProgress.Stages.Select((s, i) => new TrackingStageDto
        {
            Stage     = s,
            IsDone     = i < reachedIndex || (i == reachedIndex && !isRejected),
            IsCurrent  = i == reachedIndex
        }).ToList();

        return new TrackingStatusDto
        {
            CandidateName   = candidate.Name,
            JobTitle        = candidate.JobOpening.Title,
            CompanyName     = candidate.JobOpening.Organization.Name,
            CurrentStage    = candidate.Stage,
            IsRejected      = isRejected,
            IsHired         = isHired,
            ProgressPercent = progressPercent,
            AppliedDate     = candidate.CreatedDate,
            Stages          = stages,
            Timeline = history.Select(h => new StageHistoryDto
            {
                Stage         = h.Stage,
                Notes         = h.Notes,
                ChangedByName = h.ChangedByName,
                ChangedAt     = h.CreatedDate
            }).ToList()
        };
    }

    private async Task<Organization> GetOwnedOrgAsync(string orgCode) =>
        await _repo.GetOrgByCodeAsync(orgCode)
            ?? throw new KeyNotFoundException("Company not found.");

    private static PublicJobOpeningDto MapToDto(JobOpening j) => new()
    {
        Id                 = j.Id,
        Title              = j.Title,
        DepartmentName     = j.Department?.Name ?? string.Empty,
        Location           = j.Location,
        EmploymentType     = j.EmploymentType,
        MinExperienceYears = j.MinExperienceYears,
        MaxExperienceYears = j.MaxExperienceYears,
        Vacancies          = j.Vacancies,
        PostedDate         = j.PostedDate
    };
}
