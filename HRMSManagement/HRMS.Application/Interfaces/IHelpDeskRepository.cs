using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IHelpDeskRepository
{
    Task<List<HelpDeskTicket>> GetAllByOrgAsync(int orgId, string? status, string? category);
    Task<List<HelpDeskTicket>> GetByRaiserAsync(int orgId, int employeeId);
    Task<HelpDeskTicket?> GetByIdAsync(int id);
    Task<int> GetNextTicketNumberAsync(int orgId);
    void Add(HelpDeskTicket ticket);
    void Update(HelpDeskTicket ticket);
    void AddComment(TicketComment comment);
    Task SaveChangesAsync();
}
