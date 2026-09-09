using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class OrganizationRepository(AppDbContext db) : IOrganizationRepository
{
    public async Task<IEnumerable<Organization>> GetAllAsync()
        => await db.Organizations.Include(o => o.Users).OrderBy(o => o.Name).ToListAsync();

    public async Task<Organization?> GetByIdAsync(int id)
        => await db.Organizations.Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Organization> CreateAsync(Organization org)
    {
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return org;
    }

    public async Task UpdateAsync(Organization org)
    {
        db.Organizations.Update(org);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var org = await db.Organizations.FindAsync(id);
        if (org != null) { db.Organizations.Remove(org); await db.SaveChangesAsync(); }
    }

    public async Task<int> GetUserCountAsync(int organizationId)
        => await db.Users.CountAsync(u => u.OrganizationId == organizationId);

    public async Task<IEnumerable<User>> GetOrgUsersAsync(int organizationId)
        => await db.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Where(u => u.OrganizationId == organizationId)
            .OrderBy(u => u.Name)
            .ToListAsync();

    public async Task<bool> EmailExistsAsync(string email)
        => await db.Users.AnyAsync(u => u.Email == email);

    public async Task<User> CreateUserAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return await db.Users.Include(u => u.Role).Include(u => u.Branch).FirstAsync(u => u.Id == user.Id);
    }

    public async Task<OrgReportDto> GetOrgReportAsync(int orgId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var orders = await db.Orders
            .Include(o => o.Cashier)
            .Where(o => o.Cashier != null && o.Cashier.OrganizationId == orgId)
            .ToListAsync();

        var activeUsers = await db.Users
            .CountAsync(u => u.OrganizationId == orgId && u.IsActive);

        return new OrgReportDto
        {
            OrganizationId   = orgId,
            TotalOrders      = orders.Count,
            TotalRevenue     = orders.Sum(o => o.GrandTotal),
            ActiveUsers      = activeUsers,
            TodayOrders      = orders.Count(o => o.OrderDate.Date == today),
            TodayRevenue     = orders.Where(o => o.OrderDate.Date == today).Sum(o => o.GrandTotal),
            ThisMonthOrders  = orders.Count(o => o.OrderDate >= monthStart),
            ThisMonthRevenue = orders.Where(o => o.OrderDate >= monthStart).Sum(o => o.GrandTotal),
        };
    }
}
