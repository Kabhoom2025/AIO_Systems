using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class RecruitmentService : IRecruitmentService
{
    private static readonly string[] ValidStages =
        { "Applied", "Screening", "Interview", "Offered", "Hired", "Rejected" };

    private static readonly string[] ValidInterviewOutcomes = { "Completed", "Cancelled", "NoShow" };

    private readonly IRecruitmentRepository _repo;

    public RecruitmentService(IRecruitmentRepository repo) => _repo = repo;

    // Job openings
    public async Task<List<JobOpeningDto>> GetOpeningsAsync(int orgId, string? status)
    {
        var openings = await _repo.GetOpeningsAsync(orgId, status);
        return openings.Select(MapOpeningToDto).ToList();
    }

    public async Task<JobOpeningDto> GetOpeningAsync(int orgId, int id)
    {
        var opening = await GetOwnedOpeningAsync(orgId, id);
        return MapOpeningToDto(opening);
    }

    public async Task<JobOpeningDto> CreateOpeningAsync(int orgId, CreateJobOpeningDto dto)
    {
        var opening = new JobOpening
        {
            OrganizationId     = orgId,
            DepartmentId       = dto.DepartmentId,
            DesignationId      = dto.DesignationId,
            Title              = dto.Title,
            Description        = dto.Description,
            Vacancies          = dto.Vacancies,
            Location           = dto.Location,
            EmploymentType     = dto.EmploymentType,
            MinExperienceYears = dto.MinExperienceYears,
            MaxExperienceYears = dto.MaxExperienceYears,
            SalaryRangeFrom    = dto.SalaryRangeFrom,
            SalaryRangeTo      = dto.SalaryRangeTo,
            Status             = "Open",
            PostedDate         = dto.PostedDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : dto.PostedDate,
            ClosingDate        = dto.ClosingDate
        };
        _repo.AddOpening(opening);
        await _repo.SaveChangesAsync();

        var created = await GetOwnedOpeningAsync(orgId, opening.Id);
        return MapOpeningToDto(created);
    }

    public async Task<JobOpeningDto> UpdateOpeningAsync(int orgId, int id, UpdateJobOpeningDto dto)
    {
        var opening = await GetOwnedOpeningAsync(orgId, id);

        opening.DepartmentId       = dto.DepartmentId;
        opening.DesignationId      = dto.DesignationId;
        opening.Title              = dto.Title;
        opening.Description        = dto.Description;
        opening.Vacancies          = dto.Vacancies;
        opening.Location           = dto.Location;
        opening.EmploymentType     = dto.EmploymentType;
        opening.MinExperienceYears = dto.MinExperienceYears;
        opening.MaxExperienceYears = dto.MaxExperienceYears;
        opening.SalaryRangeFrom    = dto.SalaryRangeFrom;
        opening.SalaryRangeTo      = dto.SalaryRangeTo;
        opening.Status             = dto.Status;
        opening.PostedDate         = dto.PostedDate;
        opening.ClosingDate        = dto.ClosingDate;
        opening.UpdatedDate        = DateTime.UtcNow;

        _repo.UpdateOpening(opening);
        await _repo.SaveChangesAsync();
        return MapOpeningToDto(opening);
    }

    public async Task DeleteOpeningAsync(int orgId, int id)
    {
        var opening = await GetOwnedOpeningAsync(orgId, id);

        if (opening.Candidates.Count > 0)
            throw new InvalidOperationException("Cannot delete a job opening that has candidates.");

        _repo.RemoveOpening(opening);
        await _repo.SaveChangesAsync();
    }

    // Candidates
    public async Task<List<CandidateDto>> GetCandidatesAsync(int orgId, int openingId)
    {
        await GetOwnedOpeningAsync(orgId, openingId);
        var candidates = await _repo.GetCandidatesByOpeningAsync(orgId, openingId);
        return candidates.Select(MapCandidateToDto).ToList();
    }

    public async Task<CandidateDetailDto> GetCandidateAsync(int orgId, int id)
    {
        var candidate = await GetOwnedCandidateAsync(orgId, id);
        var history = await _repo.GetStageHistoryAsync(candidate.Id);
        return MapCandidateToDetailDto(candidate, history);
    }

    public async Task<CandidateDto> CreateCandidateAsync(int orgId, int openingId, CreateCandidateDto dto)
    {
        var opening = await GetOwnedOpeningAsync(orgId, openingId);

        var candidate = new Candidate
        {
            OrganizationId       = orgId,
            JobOpeningId         = opening.Id,
            Name                 = dto.Name,
            Email                = dto.Email,
            Phone                = dto.Phone,
            ResumeUrl            = dto.ResumeUrl,
            CurrentCompany       = dto.CurrentCompany,
            TotalExperienceYears = dto.TotalExperienceYears,
            ExpectedSalary       = dto.ExpectedSalary,
            Source               = dto.Source,
            Rating               = dto.Rating,
            Notes                = dto.Notes,
            Stage                = "Applied",
            TrackingToken        = Guid.NewGuid().ToString("N")
        };
        _repo.AddCandidate(candidate);
        await _repo.SaveChangesAsync();

        _repo.AddStageHistory(new CandidateStageHistory
        {
            CandidateId   = candidate.Id,
            Stage         = "Applied",
            Notes         = "Added by HR.",
            ChangedByName = "HR Team"
        });
        await _repo.SaveChangesAsync();

        candidate.JobOpening = opening;
        return MapCandidateToDto(candidate);
    }

    public async Task<CandidateDto> UpdateCandidateAsync(int orgId, int id, UpdateCandidateDto dto)
    {
        var candidate = await GetOwnedCandidateAsync(orgId, id);

        candidate.Name                 = dto.Name;
        candidate.Email                = dto.Email;
        candidate.Phone                = dto.Phone;
        candidate.ResumeUrl            = dto.ResumeUrl;
        candidate.CurrentCompany       = dto.CurrentCompany;
        candidate.TotalExperienceYears = dto.TotalExperienceYears;
        candidate.ExpectedSalary       = dto.ExpectedSalary;
        candidate.Source               = dto.Source;
        candidate.Rating               = dto.Rating;
        candidate.Notes                = dto.Notes;
        candidate.UpdatedDate          = DateTime.UtcNow;

        _repo.UpdateCandidate(candidate);
        await _repo.SaveChangesAsync();
        return MapCandidateToDto(candidate);
    }

    public async Task<CandidateDto> ChangeStageAsync(int orgId, int id, ChangeCandidateStageDto dto, string changedByName)
    {
        if (!ValidStages.Contains(dto.Stage))
            throw new InvalidOperationException(
                $"Invalid stage '{dto.Stage}'. Valid stages are: {string.Join(", ", ValidStages)}.");

        var candidate = await GetOwnedCandidateAsync(orgId, id);

        candidate.Stage       = dto.Stage;
        candidate.Notes       = dto.Notes ?? candidate.Notes;
        candidate.UpdatedDate = DateTime.UtcNow;

        if (dto.Stage == "Offered")
        {
            candidate.OfferedSalary        = dto.OfferedSalary;
            candidate.OfferDate            = dto.OfferDate;
            candidate.ExpectedJoiningDate  = dto.ExpectedJoiningDate;
        }

        _repo.UpdateCandidate(candidate);
        _repo.AddStageHistory(new CandidateStageHistory
        {
            CandidateId   = candidate.Id,
            Stage         = dto.Stage,
            Notes         = dto.Notes,
            ChangedByName = string.IsNullOrWhiteSpace(changedByName) ? "HR Team" : changedByName
        });
        await _repo.SaveChangesAsync();
        return MapCandidateToDto(candidate);
    }

    public async Task DeleteCandidateAsync(int orgId, int id)
    {
        var candidate = await GetOwnedCandidateAsync(orgId, id);
        _repo.RemoveCandidate(candidate);
        await _repo.SaveChangesAsync();
    }

    // Interviews
    public async Task<List<InterviewDto>> GetUpcomingInterviewsAsync(int orgId)
    {
        var interviews = await _repo.GetUpcomingInterviewsAsync(orgId);
        return interviews.Select(MapInterviewToDto).ToList();
    }

    public async Task<InterviewDto> ScheduleInterviewAsync(int orgId, int candidateId, CreateInterviewDto dto)
    {
        var candidate = await GetOwnedCandidateAsync(orgId, candidateId);

        var interview = new Interview
        {
            CandidateId       = candidate.Id,
            Round             = dto.Round,
            Title             = dto.Title,
            ScheduledAt       = dto.ScheduledAt,
            Mode              = dto.Mode,
            InterviewerName   = dto.InterviewerName,
            InterviewerUserId = dto.InterviewerUserId,
            Status            = "Scheduled"
        };
        _repo.AddInterview(interview);
        await _repo.SaveChangesAsync();

        interview.Candidate = candidate;
        return MapInterviewToDto(interview);
    }

    public async Task<InterviewDto> UpdateInterviewAsync(int orgId, int id, UpdateInterviewDto dto)
    {
        var interview = await GetOwnedInterviewAsync(orgId, id);

        interview.Round             = dto.Round;
        interview.Title             = dto.Title;
        interview.ScheduledAt       = dto.ScheduledAt;
        interview.Mode              = dto.Mode;
        interview.InterviewerName   = dto.InterviewerName;
        interview.InterviewerUserId = dto.InterviewerUserId;
        interview.UpdatedDate       = DateTime.UtcNow;

        _repo.UpdateInterview(interview);
        await _repo.SaveChangesAsync();
        return MapInterviewToDto(interview);
    }

    public async Task<InterviewDto> SubmitInterviewFeedbackAsync(int orgId, int id, InterviewFeedbackDto dto)
    {
        if (!ValidInterviewOutcomes.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Invalid interview status '{dto.Status}'. Valid values are: {string.Join(", ", ValidInterviewOutcomes)}.");

        var interview = await GetOwnedInterviewAsync(orgId, id);

        interview.Status      = dto.Status;
        interview.Feedback    = dto.Feedback;
        interview.Score       = dto.Score;
        interview.UpdatedDate = DateTime.UtcNow;

        _repo.UpdateInterview(interview);
        await _repo.SaveChangesAsync();
        return MapInterviewToDto(interview);
    }

    // Pipeline
    public async Task<RecruitmentPipelineDto> GetPipelineAsync(int orgId)
    {
        var counts = await _repo.GetPipelineCountsAsync(orgId);
        return new RecruitmentPipelineDto
        {
            Applied   = counts.GetValueOrDefault("Applied"),
            Screening = counts.GetValueOrDefault("Screening"),
            Interview = counts.GetValueOrDefault("Interview"),
            Offered   = counts.GetValueOrDefault("Offered"),
            Hired     = counts.GetValueOrDefault("Hired"),
            Rejected  = counts.GetValueOrDefault("Rejected")
        };
    }

    private async Task<JobOpening> GetOwnedOpeningAsync(int orgId, int id) =>
        await _repo.GetOpeningAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Job opening {id} not found");

    private async Task<Candidate> GetOwnedCandidateAsync(int orgId, int id) =>
        await _repo.GetCandidateAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Candidate {id} not found");

    private async Task<Interview> GetOwnedInterviewAsync(int orgId, int id) =>
        await _repo.GetInterviewAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Interview {id} not found");

    private static JobOpeningDto MapOpeningToDto(JobOpening j) => new()
    {
        Id                 = j.Id,
        DepartmentId       = j.DepartmentId,
        DepartmentName     = j.Department?.Name ?? string.Empty,
        DesignationId      = j.DesignationId,
        DesignationTitle   = j.Designation?.Title ?? string.Empty,
        Title              = j.Title,
        Description        = j.Description,
        Vacancies          = j.Vacancies,
        Location           = j.Location,
        EmploymentType     = j.EmploymentType,
        MinExperienceYears = j.MinExperienceYears,
        MaxExperienceYears = j.MaxExperienceYears,
        SalaryRangeFrom    = j.SalaryRangeFrom,
        SalaryRangeTo      = j.SalaryRangeTo,
        Status             = j.Status,
        PostedDate         = j.PostedDate,
        ClosingDate        = j.ClosingDate,
        CandidateCount     = j.Candidates.Count
    };

    private static T FillCandidateDto<T>(T dto, Candidate c) where T : CandidateDto
    {
        dto.Id                   = c.Id;
        dto.JobOpeningId         = c.JobOpeningId;
        dto.JobOpeningTitle      = c.JobOpening?.Title ?? string.Empty;
        dto.Name                 = c.Name;
        dto.Email                = c.Email;
        dto.Phone                = c.Phone;
        dto.ResumeUrl            = c.ResumeUrl;
        dto.CurrentCompany       = c.CurrentCompany;
        dto.TotalExperienceYears = c.TotalExperienceYears;
        dto.ExpectedSalary       = c.ExpectedSalary;
        dto.Source               = c.Source;
        dto.Stage                = c.Stage;
        dto.Rating               = c.Rating;
        dto.OfferedSalary        = c.OfferedSalary;
        dto.OfferDate            = c.OfferDate;
        dto.ExpectedJoiningDate  = c.ExpectedJoiningDate;
        dto.Notes                = c.Notes;
        return dto;
    }

    private static CandidateDto MapCandidateToDto(Candidate c) => FillCandidateDto(new CandidateDto(), c);

    private static CandidateDetailDto MapCandidateToDetailDto(Candidate c, List<CandidateStageHistory> history)
    {
        var dto = FillCandidateDto(new CandidateDetailDto(), c);
        dto.Interviews = c.Interviews
            .OrderByDescending(i => i.ScheduledAt)
            .Select(MapInterviewToDto)
            .ToList();
        dto.StageHistory = history.Select(MapStageHistoryToDto).ToList();
        dto.ProgressPercent = CandidateStageProgress.GetProgressPercent(c.Stage, history.Select(h => h.Stage));
        return dto;
    }

    private static StageHistoryDto MapStageHistoryToDto(CandidateStageHistory h) => new()
    {
        Stage         = h.Stage,
        Notes         = h.Notes,
        ChangedByName = h.ChangedByName,
        ChangedAt     = h.CreatedDate
    };

    private static InterviewDto MapInterviewToDto(Interview i) => new()
    {
        Id                = i.Id,
        CandidateId       = i.CandidateId,
        CandidateName     = i.Candidate?.Name ?? string.Empty,
        JobOpeningTitle   = i.Candidate?.JobOpening?.Title ?? string.Empty,
        Round             = i.Round,
        Title             = i.Title,
        ScheduledAt       = i.ScheduledAt,
        Mode              = i.Mode,
        InterviewerName   = i.InterviewerName,
        InterviewerUserId = i.InterviewerUserId,
        Status            = i.Status,
        Feedback          = i.Feedback,
        Score             = i.Score
    };
}
