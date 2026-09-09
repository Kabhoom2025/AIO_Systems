using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly PharmacyDbContext _ctx;

    public ExpenseRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Expense>> GetAllByOrgAsync(int orgId) =>
        _ctx.Expenses
            .Include(e => e.Branch)
            .Where(e => e.OrganizationId == orgId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

    public Task<Expense?> GetByIdAsync(int id) =>
        _ctx.Expenses.Include(e => e.Branch).FirstOrDefaultAsync(e => e.Id == id);

    public void Add(Expense expense)    => _ctx.Expenses.Add(expense);
    public void Update(Expense expense) => _ctx.Expenses.Update(expense);
    public void Remove(Expense expense) => _ctx.Expenses.Remove(expense);
    public Task SaveChangesAsync()      => _ctx.SaveChangesAsync();
}
