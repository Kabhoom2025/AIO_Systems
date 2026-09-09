using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IPerformanceRepository
{
    Task<bool> EmployeeExistsAsync(int orgId, int employeeId);

    // Goals
    Task<List<PerformanceGoal>> GetGoalsByEmployeeAsync(int orgId, int employeeId);
    Task<PerformanceGoal?> GetGoalAsync(int orgId, int id);
    void AddGoal(PerformanceGoal goal);
    void UpdateGoal(PerformanceGoal goal);
    void RemoveGoal(PerformanceGoal goal);

    // Reviews
    Task<List<PerformanceReview>> GetReviewsAsync(int orgId, string? period, string? status);
    Task<List<PerformanceReview>> GetReviewsByEmployeeAsync(int orgId, int employeeId);
    Task<PerformanceReview?> GetReviewAsync(int orgId, int id);
    Task<bool> ReviewExistsForPeriodAsync(int orgId, int employeeId, string period);
    void AddReview(PerformanceReview review);
    void UpdateReview(PerformanceReview review);
    void RemoveReview(PerformanceReview review);

    Task SaveChangesAsync();
}
