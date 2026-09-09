using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class ShiftRepository : IShiftRepository
{
    private readonly HrmsDbContext _ctx;

    public ShiftRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<Shift>> GetAllByOrgAsync(int orgId) =>
        _ctx.Shifts
            .Where(s => s.OrganizationId == orgId)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public Task<Shift?> GetByIdAsync(int orgId, int id) =>
        _ctx.Shifts.FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId);

    public void Add(Shift shift)    => _ctx.Shifts.Add(shift);
    public void Update(Shift shift) => _ctx.Shifts.Update(shift);
    public void Remove(Shift shift) => _ctx.Shifts.Remove(shift);
    public Task SaveChangesAsync()  => _ctx.SaveChangesAsync();
}
