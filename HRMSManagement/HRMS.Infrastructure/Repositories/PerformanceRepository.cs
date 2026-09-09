using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class PerformanceRepository : IPerformanceRepository
{
    private readonly HrmsDbContext _ctx;

    public PerformanceRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<bool> EmployeeExistsAsync(int orgId, int employeeId) =>
        _ctx.Employees.AnyAsync(e => e.OrganizationId == orgId && e.Id == employeeId);

    // Goals
    public Task<List<PerformanceGoal>> GetGoalsByEmployeeAsync(int orgId, int employeeId) =>
        _ctx.PerformanceGoals
            .Include(g => g.Employee)
            .Where(g => g.OrganizationId == orgId && g.EmployeeId == employeeId)
            .OrderByDescending(g => g.DueDate)
            .ToListAsync();

    public Task<PerformanceGoal?> GetGoalAsync(int orgId, int id) =>
        _ctx.PerformanceGoals
            .Include(g => g.Employee)
            .FirstOrDefaultAsync(g => g.OrganizationId == orgId && g.Id == id);

    public void AddGoal(PerformanceGoal goal)    => _ctx.PerformanceGoals.Add(goal);
    public void UpdateGoal(PerformanceGoal goal) => _ctx.PerformanceGoals.Update(goal);
    public void RemoveGoal(PerformanceGoal goal) => _ctx.PerformanceGoals.Remove(goal);

    // Reviews
    public Task<List<PerformanceReview>> GetReviewsAsync(int orgId, string? period, string? status)
    {
        var query = _ctx.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.ReviewerUser)
            .Where(r => r.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(period))
            query = query.Where(r => r.Period == period);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        return query.OrderByDescending(r => r.CreatedDate).ToListAsync();
    }

    public Task<List<PerformanceReview>> GetReviewsByEmployeeAsync(int orgId, int employeeId) =>
        _ctx.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.ReviewerUser)
            .Where(r => r.OrganizationId == orgId && r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public Task<PerformanceReview?> GetReviewAsync(int orgId, int id) =>
        _ctx.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.ReviewerUser)
            .FirstOrDefaultAsync(r => r.OrganizationId == orgId && r.Id == id);

    public Task<bool> ReviewExistsForPeriodAsync(int orgId, int employeeId, string period) =>
        _ctx.PerformanceReviews.AnyAsync(r =>
            r.OrganizationId == orgId && r.EmployeeId == employeeId && r.Period == period);

    public void AddReview(PerformanceReview review)    => _ctx.PerformanceReviews.Add(review);
    public void UpdateReview(PerformanceReview review) => _ctx.PerformanceReviews.Update(review);
    public void RemoveReview(PerformanceReview review) => _ctx.PerformanceReviews.Remove(review);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
