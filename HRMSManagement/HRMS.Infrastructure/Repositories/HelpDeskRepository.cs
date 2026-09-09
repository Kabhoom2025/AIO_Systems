using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class HelpDeskRepository : IHelpDeskRepository
{
    private readonly HrmsDbContext _ctx;

    public HelpDeskRepository(HrmsDbContext ctx) => _ctx = ctx;

    public Task<List<HelpDeskTicket>> GetAllByOrgAsync(int orgId, string? status, string? category)
    {
        var query = _ctx.HelpDeskTickets
            .Include(t => t.RaisedByEmployee)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .Where(t => t.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category);

        return query.OrderByDescending(t => t.CreatedDate).ToListAsync();
    }

    public Task<List<HelpDeskTicket>> GetByRaiserAsync(int orgId, int employeeId) =>
        _ctx.HelpDeskTickets
            .Include(t => t.RaisedByEmployee)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .Where(t => t.OrganizationId == orgId && t.RaisedByEmployeeId == employeeId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();

    public Task<HelpDeskTicket?> GetByIdAsync(int id) =>
        _ctx.HelpDeskTickets
            .Include(t => t.RaisedByEmployee)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments).ThenInclude(c => c.AuthorUser)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<int> GetNextTicketNumberAsync(int orgId)
    {
        var numbers = await _ctx.HelpDeskTickets
            .Where(t => t.OrganizationId == orgId)
            .Select(t => t.TicketNumber)
            .ToListAsync();

        var max = 0;
        foreach (var number in numbers)
        {
            var numberPart = number.Contains('-') ? number[(number.LastIndexOf('-') + 1)..] : number;
            if (int.TryParse(numberPart, out var n) && n > max)
                max = n;
        }
        return max + 1;
    }

    public void Add(HelpDeskTicket ticket)    => _ctx.HelpDeskTickets.Add(ticket);
    public void Update(HelpDeskTicket ticket) => _ctx.HelpDeskTickets.Update(ticket);
    public void AddComment(TicketComment comment) => _ctx.TicketComments.Add(comment);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
