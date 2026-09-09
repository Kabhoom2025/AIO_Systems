using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class LicenseRepository(AppDbContext db) : ILicenseRepository
{
    public async Task<IEnumerable<LicenseDto>> GetAllAsync()
        => await db.Licenses
            .Include(l => l.Organization)
            .OrderBy(l => l.Organization.Name)
            .Select(l => new LicenseDto
            {
                Id             = l.Id,
                OrganizationId = l.OrganizationId,
                OrgName        = l.Organization.Name,
                Plan           = l.Plan,
                Status         = l.Status,
                ExpiryDate     = l.ExpiryDate,
                MaxUsers       = l.MaxUsers,
                Notes          = l.Notes,
                CreatedDate    = l.CreatedDate,
            })
            .ToListAsync();

    public async Task<License?> GetByOrgAsync(int orgId)
        => await db.Licenses.Include(l => l.Organization)
            .FirstOrDefaultAsync(l => l.OrganizationId == orgId);

    public async Task<License> UpsertAsync(int orgId, UpsertLicenseRequest request)
    {
        var existing = await db.Licenses.FirstOrDefaultAsync(l => l.OrganizationId == orgId);

        if (existing == null)
        {
            existing = new License { OrganizationId = orgId };
            db.Licenses.Add(existing);
        }

        existing.Plan       = request.Plan;
        existing.Status     = request.Status;
        existing.ExpiryDate = request.ExpiryDate;
        existing.MaxUsers   = request.MaxUsers;
        existing.Notes      = request.Notes;

        await db.SaveChangesAsync();
        return await db.Licenses.Include(l => l.Organization).FirstAsync(l => l.Id == existing.Id);
    }

    public async Task DeleteAsync(int orgId)
    {
        var license = await db.Licenses.FirstOrDefaultAsync(l => l.OrganizationId == orgId);
        if (license != null) { db.Licenses.Remove(license); await db.SaveChangesAsync(); }
    }
}
