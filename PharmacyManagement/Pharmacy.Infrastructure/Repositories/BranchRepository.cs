using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly PharmacyDbContext _ctx;

    public BranchRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Branch>> GetAllByOrgAsync(int orgId) =>
        _ctx.Branches
            .Where(b => b.OrganizationId == orgId)
            .OrderBy(b => b.Name)
            .ToListAsync();

    public Task<Branch?> GetByIdAsync(int id) =>
        _ctx.Branches.FirstOrDefaultAsync(b => b.Id == id);

    public void Add(Branch branch)    => _ctx.Branches.Add(branch);
    public void Update(Branch branch) => _ctx.Branches.Update(branch);
    public void Remove(Branch branch) => _ctx.Branches.Remove(branch);
    public Task SaveChangesAsync()    => _ctx.SaveChangesAsync();
}
