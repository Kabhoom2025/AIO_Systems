using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class PerformanceService : IPerformanceService
{
    private static readonly string[] ValidGoalStatuses = { "NotStarted", "InProgress", "Completed", "Cancelled" };

    private readonly IPerformanceRepository _repo;

    public PerformanceService(IPerformanceRepository repo) => _repo = repo;

    // Goals
    public async Task<List<PerformanceGoalDto>> GetGoalsByEmployeeAsync(int orgId, int employeeId)
    {
        var goals = await _repo.GetGoalsByEmployeeAsync(orgId, employeeId);
        return goals.Select(MapGoalToDto).ToList();
    }

    public async Task<PerformanceGoalDto> CreateGoalAsync(int orgId, CreatePerformanceGoalDto dto)
    {
        if (!await _repo.EmployeeExistsAsync(orgId, dto.EmployeeId))
            throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found");

        var goal = new PerformanceGoal
        {
            OrganizationId  = orgId,
            EmployeeId      = dto.EmployeeId,
            Title           = dto.Title,
            Description     = dto.Description,
            Metric          = dto.Metric,
            Weight          = dto.Weight,
            StartDate       = dto.StartDate,
            DueDate         = dto.DueDate,
            ProgressPercent = 0,
            Status          = "NotStarted"
        };
        _repo.AddGoal(goal);
        await _repo.SaveChangesAsync();

        var created = await GetOwnedGoalAsync(orgId, goal.Id);
        return MapGoalToDto(created);
    }

    public async Task<PerformanceGoalDto> UpdateGoalAsync(int orgId, int id, UpdatePerformanceGoalDto dto)
    {
        if (!ValidGoalStatuses.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Invalid status '{dto.Status}'. Valid values are: {string.Join(", ", ValidGoalStatuses)}.");

        var goal = await GetOwnedGoalAsync(orgId, id);

        goal.Title       = dto.Title;
        goal.Description = dto.Description;
        goal.Metric       = dto.Metric;
        goal.Weight       = dto.Weight;
        goal.StartDate    = dto.StartDate;
        goal.DueDate      = dto.DueDate;
        goal.Status       = dto.Status;
        goal.UpdatedDate  = DateTime.UtcNow;

        _repo.UpdateGoal(goal);
        await _repo.SaveChangesAsync();
        return MapGoalToDto(goal);
    }

    public async Task<PerformanceGoalDto> UpdateGoalProgressAsync(
        int orgId, int id, int? callerEmployeeId, bool callerCanEdit, UpdateGoalProgressDto dto)
    {
        var goal = await GetOwnedGoalAsync(orgId, id);

        if (!callerCanEdit && goal.EmployeeId != callerEmployeeId)
            throw new UnauthorizedAccessException("You are not allowed to update progress for this goal.");

        if (dto.ProgressPercent is < 0 or > 100)
            throw new InvalidOperationException("ProgressPercent must be between 0 and 100.");

        goal.ProgressPercent = dto.ProgressPercent;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!ValidGoalStatuses.Contains(dto.Status))
                throw new InvalidOperationException(
                    $"Invalid status '{dto.Status}'. Valid values are: {string.Join(", ", ValidGoalStatuses)}.");
            goal.Status = dto.Status;
        }

        goal.UpdatedDate = DateTime.UtcNow;

        _repo.UpdateGoal(goal);
        await _repo.SaveChangesAsync();
        return MapGoalToDto(goal);
    }

    public async Task DeleteGoalAsync(int orgId, int id)
    {
        var goal = await GetOwnedGoalAsync(orgId, id);
        _repo.RemoveGoal(goal);
        await _repo.SaveChangesAsync();
    }

    // Reviews
    public async Task<List<PerformanceReviewDto>> GetReviewsAsync(int orgId, string? period, string? status)
    {
        var reviews = await _repo.GetReviewsAsync(orgId, period, status);
        return reviews.Select(MapReviewToDto).ToList();
    }

    public async Task<List<PerformanceReviewDto>> GetReviewsByEmployeeAsync(int orgId, int employeeId)
    {
        var reviews = await _repo.GetReviewsByEmployeeAsync(orgId, employeeId);
        return reviews.Select(MapReviewToDto).ToList();
    }

    public async Task<PerformanceReviewDto> GetReviewAsync(int orgId, int id)
    {
        var review = await GetOwnedReviewAsync(orgId, id);
        return MapReviewToDto(review);
    }

    public async Task<PerformanceReviewDto> CreateReviewAsync(int orgId, CreatePerformanceReviewDto dto)
    {
        if (!await _repo.EmployeeExistsAsync(orgId, dto.EmployeeId))
            throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found");

        if (await _repo.ReviewExistsForPeriodAsync(orgId, dto.EmployeeId, dto.Period))
            throw new InvalidOperationException(
                $"A performance review for employee {dto.EmployeeId} in period '{dto.Period}' already exists.");

        var review = new PerformanceReview
        {
            OrganizationId = orgId,
            EmployeeId     = dto.EmployeeId,
            ReviewerUserId = dto.ReviewerUserId,
            Period         = dto.Period,
            Status         = "SelfReview",
            Recommendation = "None"
        };
        _repo.AddReview(review);
        await _repo.SaveChangesAsync();

        var created = await GetOwnedReviewAsync(orgId, review.Id);
        return MapReviewToDto(created);
    }

    public async Task<PerformanceReviewDto> SubmitSelfReviewAsync(int orgId, int id, int employeeId, SelfReviewDto dto)
    {
        var review = await GetOwnedReviewAsync(orgId, id);

        if (review.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only submit your own performance review.");
        if (review.Status != "SelfReview")
            throw new InvalidOperationException("This review is not awaiting self-review.");
        if (dto.SelfRating is < 1 or > 5)
            throw new InvalidOperationException("SelfRating must be between 1 and 5.");

        review.SelfRating   = dto.SelfRating;
        review.SelfComments = dto.SelfComments;
        review.Status       = "ManagerReview";
        review.UpdatedDate  = DateTime.UtcNow;

        _repo.UpdateReview(review);
        await _repo.SaveChangesAsync();
        return MapReviewToDto(review);
    }

    public async Task<PerformanceReviewDto> SubmitManagerReviewAsync(int orgId, int id, ManagerReviewDto dto)
    {
        var review = await GetOwnedReviewAsync(orgId, id);

        if (dto.ManagerRating is < 1 or > 5)
            throw new InvalidOperationException("ManagerRating must be between 1 and 5.");

        review.ManagerRating   = dto.ManagerRating;
        review.ManagerComments = dto.ManagerComments;
        review.Recommendation  = dto.Recommendation;
        review.FinalRating     = review.SelfRating.HasValue
            ? Math.Round((review.SelfRating.Value + dto.ManagerRating) / 2, 1)
            : Math.Round(dto.ManagerRating, 1);
        review.Status       = "Completed";
        review.CompletedAt  = DateTime.UtcNow;
        review.UpdatedDate  = DateTime.UtcNow;

        _repo.UpdateReview(review);
        await _repo.SaveChangesAsync();
        return MapReviewToDto(review);
    }

    public async Task DeleteReviewAsync(int orgId, int id)
    {
        var review = await GetOwnedReviewAsync(orgId, id);
        _repo.RemoveReview(review);
        await _repo.SaveChangesAsync();
    }

    private async Task<PerformanceGoal> GetOwnedGoalAsync(int orgId, int id) =>
        await _repo.GetGoalAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Performance goal {id} not found");

    private async Task<PerformanceReview> GetOwnedReviewAsync(int orgId, int id) =>
        await _repo.GetReviewAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Performance review {id} not found");

    private static PerformanceGoalDto MapGoalToDto(PerformanceGoal g) => new()
    {
        Id              = g.Id,
        EmployeeId      = g.EmployeeId,
        EmployeeName    = g.Employee?.FullName ?? string.Empty,
        Title           = g.Title,
        Description     = g.Description,
        Metric          = g.Metric,
        Weight          = g.Weight,
        StartDate       = g.StartDate,
        DueDate         = g.DueDate,
        ProgressPercent = g.ProgressPercent,
        Status          = g.Status
    };

    private static PerformanceReviewDto MapReviewToDto(PerformanceReview r) => new()
    {
        Id              = r.Id,
        EmployeeId      = r.EmployeeId,
        EmployeeName    = r.Employee?.FullName ?? string.Empty,
        ReviewerUserId  = r.ReviewerUserId,
        ReviewerName    = r.ReviewerUser?.Name,
        Period          = r.Period,
        SelfRating      = r.SelfRating,
        SelfComments    = r.SelfComments,
        ManagerRating   = r.ManagerRating,
        ManagerComments = r.ManagerComments,
        FinalRating     = r.FinalRating,
        Status          = r.Status,
        Recommendation  = r.Recommendation,
        CompletedAt     = r.CompletedAt
    };
}
