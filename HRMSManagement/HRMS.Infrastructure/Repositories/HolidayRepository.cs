using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class HolidayRepository : IHolidayRepository
{
    private readonly HrmsDbContext _ctx;

    public HolidayRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<Holiday>> GetAllByOrgAsync(int orgId, int year)
    {
        var start = new DateOnly(year, 1, 1);
        var end   = new DateOnly(year, 12, 31);

        return _ctx.Holidays
            .Include(h => h.Branch)
            .Where(h => h.OrganizationId == orgId && h.Date >= start && h.Date <= end)
            .OrderBy(h => h.Date)
            .ToListAsync();
    }

    public Task<List<Holiday>> GetUpcomingAsync(int orgId, DateOnly fromDate, int count) =>
        _ctx.Holidays
            .Include(h => h.Branch)
            .Where(h => h.OrganizationId == orgId && h.Date >= fromDate)
            .OrderBy(h => h.Date)
            .Take(count)
            .ToListAsync();

    public Task<Holiday?> GetByIdAsync(int orgId, int id) =>
        _ctx.Holidays
            .Include(h => h.Branch)
            .FirstOrDefaultAsync(h => h.Id == id && h.OrganizationId == orgId);

    public void Add(Holiday holiday)    => _ctx.Holidays.Add(holiday);
    public void Update(Holiday holiday) => _ctx.Holidays.Update(holiday);
    public void Remove(Holiday holiday) => _ctx.Holidays.Remove(holiday);
    public Task SaveChangesAsync()      => _ctx.SaveChangesAsync();
}
