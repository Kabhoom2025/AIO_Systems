using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly HrmsDbContext _ctx;

    public ExpenseRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<ExpenseClaim>> GetAllByOrgAsync(int orgId, string? status)
    {
        var query = _ctx.ExpenseClaims
            .Include(e => e.Employee)
            .Include(e => e.ReviewedByUser)
            .Where(e => e.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.Status == status);

        return query.OrderByDescending(e => e.ClaimDate).ToListAsync();
    }

    public Task<List<ExpenseClaim>> GetByEmployeeAsync(int orgId, int employeeId) =>
        _ctx.ExpenseClaims
            .Include(e => e.Employee)
            .Include(e => e.ReviewedByUser)
            .Where(e => e.OrganizationId == orgId && e.EmployeeId == employeeId)
            .OrderByDescending(e => e.ClaimDate)
            .ToListAsync();

    public Task<ExpenseClaim?> GetByIdAsync(int id) =>
        _ctx.ExpenseClaims
            .Include(e => e.Employee)
            .Include(e => e.ReviewedByUser)
            .FirstOrDefaultAsync(e => e.Id == id);

    public void Add(ExpenseClaim claim)    => _ctx.ExpenseClaims.Add(claim);
    public void Update(ExpenseClaim claim) => _ctx.ExpenseClaims.Update(claim);
    public void Remove(ExpenseClaim claim) => _ctx.ExpenseClaims.Remove(claim);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
