using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly HrmsDbContext _ctx;

    public BranchRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<Branch>> GetAllByOrgAsync(int orgId) =>
        _ctx.Branches
            .Where(b => b.OrganizationId == orgId)
            .OrderBy(b => b.Name)
            .ToListAsync();

    public Task<Branch?> GetByIdAsync(int orgId, int id) =>
        _ctx.Branches.FirstOrDefaultAsync(b => b.Id == id && b.OrganizationId == orgId);

    public void Add(Branch branch)    => _ctx.Branches.Add(branch);
    public void Update(Branch branch) => _ctx.Branches.Update(branch);
    public void Remove(Branch branch) => _ctx.Branches.Remove(branch);
    public Task SaveChangesAsync()    => _ctx.SaveChangesAsync();
}
