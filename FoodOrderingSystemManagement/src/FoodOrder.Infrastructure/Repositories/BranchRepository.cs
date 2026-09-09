using FoodOrder.Application.DTOs.Branch;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class BranchRepository(AppDbContext db) : IBranchRepository
{
    public async Task<IReadOnlyList<Branch>> GetByOrganizationAsync(int organizationId) =>
        await db.Branches.Where(b => b.OrganizationId == organizationId).OrderBy(b => b.Name).ToListAsync();

    public async Task<Branch?> GetByIdAsync(int id) =>
        await db.Branches.FindAsync(id);

    public async Task<Branch> CreateAsync(Branch branch)
    {
        db.Branches.Add(branch);
        await db.SaveChangesAsync();
        return branch;
    }

    public async Task UpdateAsync(Branch branch)
    {
        db.Branches.Update(branch);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await db.Branches.FindAsync(id);
        if (branch != null) { db.Branches.Remove(branch); await db.SaveChangesAsync(); }
    }

    public async Task<int?> GetDefaultBranchIdAsync(int organizationId)
    {
        var branch = await db.Branches
            .Where(b => b.OrganizationId == organizationId && b.IsDefault)
            .Select(b => (int?)b.Id)
            .FirstOrDefaultAsync();
        return branch ?? await db.Branches
            .Where(b => b.OrganizationId == organizationId)
            .OrderBy(b => b.Id)
            .Select(b => (int?)b.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<BranchReportDto>> GetBranchReportsAsync(int organizationId)
    {
        var branches = await db.Branches
            .Where(b => b.OrganizationId == organizationId)
            .OrderBy(b => b.Name)
            .ToListAsync();

        // IgnoreQueryFilters: this must see every order in the org regardless of the
        // caller's own BranchId claim — a per-branch comparison would be pointless if
        // a branch-scoped caller could only ever see their own branch's row.
        var orders = await db.Orders
            .IgnoreQueryFilters()
            .Where(o => o.Branch.OrganizationId == organizationId)
            .Select(o => new { o.BranchId, o.OrderDate, o.GrandTotal })
            .ToListAsync();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        return branches.Select(b =>
        {
            var branchOrders = orders.Where(o => o.BranchId == b.Id).ToList();
            return new BranchReportDto
            {
                BranchId = b.Id,
                BranchName = b.Name,
                TotalOrders = branchOrders.Count,
                TotalRevenue = branchOrders.Sum(o => o.GrandTotal),
                TodayOrders = branchOrders.Count(o => o.OrderDate.Date == today),
                TodayRevenue = branchOrders.Where(o => o.OrderDate.Date == today).Sum(o => o.GrandTotal),
                ThisMonthOrders = branchOrders.Count(o => o.OrderDate >= monthStart),
                ThisMonthRevenue = branchOrders.Where(o => o.OrderDate >= monthStart).Sum(o => o.GrandTotal),
            };
        }).ToList();
    }
}
