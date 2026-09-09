using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly HrmsDbContext _ctx;

    public AssetRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<Asset>> GetAllByOrgAsync(int orgId, string? category, string? status)
    {
        var query = _ctx.Assets
            .Include(a => a.Branch)
            .Include(a => a.Allocations.Where(al => al.ReturnedDate == null))
                .ThenInclude(al => al.Employee)
            .Where(a => a.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        return query.OrderBy(a => a.AssetTag).ToListAsync();
    }

    public Task<Asset?> GetByIdAsync(int id) =>
        _ctx.Assets
            .Include(a => a.Branch)
            .Include(a => a.Allocations).ThenInclude(al => al.Employee)
            .Include(a => a.Allocations).ThenInclude(al => al.AllocatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<Asset>> GetAllocatedToEmployeeAsync(int orgId, int employeeId) =>
        _ctx.Assets
            .Include(a => a.Branch)
            .Include(a => a.Allocations.Where(al => al.ReturnedDate == null))
                .ThenInclude(al => al.Employee)
            .Where(a => a.OrganizationId == orgId &&
                        a.Allocations.Any(al => al.EmployeeId == employeeId && al.ReturnedDate == null))
            .OrderBy(a => a.AssetTag)
            .ToListAsync();

    public async Task<int> GetNextTagNumberAsync(int orgId)
    {
        var tags = await _ctx.Assets
            .Where(a => a.OrganizationId == orgId)
            .Select(a => a.AssetTag)
            .ToListAsync();

        var max = 0;
        foreach (var tag in tags)
        {
            var numberPart = tag.Contains('-') ? tag[(tag.LastIndexOf('-') + 1)..] : tag;
            if (int.TryParse(numberPart, out var n) && n > max)
                max = n;
        }
        return max + 1;
    }

    public void Add(Asset asset)    => _ctx.Assets.Add(asset);
    public void Update(Asset asset) => _ctx.Assets.Update(asset);
    public void Remove(Asset asset) => _ctx.Assets.Remove(asset);
    public void AddAllocation(AssetAllocation allocation) => _ctx.AssetAllocations.Add(allocation);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
