using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class HelpDeskService : IHelpDeskService
{
    private static readonly string[] ValidStatuses = { "Open", "InProgress", "Resolved", "Closed" };

    private readonly IHelpDeskRepository _repo;

    public HelpDeskService(IHelpDeskRepository repo) => _repo = repo;

    public async Task<TicketDto> CreateTicketAsync(int orgId, int employeeId, CreateTicketDto dto)
    {
        var nextNumber = await _repo.GetNextTicketNumberAsync(orgId);
        var slaHours = dto.Priority switch
        {
            "Critical" => 4,
            "High"     => 8,
            "Medium"   => 24,
            "Low"      => 72,
            _          => 24
        };

        var ticket = new HelpDeskTicket
        {
            OrganizationId     = orgId,
            RaisedByEmployeeId = employeeId,
            TicketNumber       = $"TCK-{nextNumber:D4}",
            Category           = dto.Category,
            Priority           = dto.Priority,
            Subject            = dto.Subject,
            Description        = dto.Description,
            Status             = "Open",
            SlaDueAt           = DateTime.UtcNow.AddHours(slaHours)
        };
        _repo.Add(ticket);
        await _repo.SaveChangesAsync();
        return MapToDto(ticket);
    }

    public async Task<List<TicketDto>> GetMyTicketsAsync(int orgId, int employeeId)
    {
        var tickets = await _repo.GetByRaiserAsync(orgId, employeeId);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<List<TicketDto>> GetAllAsync(int orgId, string? status, string? category)
    {
        var tickets = await _repo.GetAllByOrgAsync(orgId, status, category);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<TicketDetailDto> GetByIdAsync(int orgId, int id, int? callerEmployeeId, bool canViewAll)
    {
        var ticket = await GetOwnedTicketAsync(orgId, id);
        EnsureCanAccess(ticket, callerEmployeeId, canViewAll);
        return MapToDetailDto(ticket);
    }

    public async Task<TicketCommentDto> AddCommentAsync(int orgId, int id, int? userId, string userName,
        int? callerEmployeeId, bool canViewAll, AddCommentDto dto)
    {
        var ticket = await GetOwnedTicketAsync(orgId, id);
        EnsureCanAccess(ticket, callerEmployeeId, canViewAll);

        var comment = new TicketComment
        {
            TicketId     = ticket.Id,
            AuthorUserId = userId,
            AuthorName   = userName,
            Comment      = dto.Comment
        };
        _repo.AddComment(comment);
        await _repo.SaveChangesAsync();

        return new TicketCommentDto
        {
            Id           = comment.Id,
            AuthorUserId = comment.AuthorUserId,
            AuthorName   = comment.AuthorName,
            Comment      = comment.Comment,
            CreatedDate  = comment.CreatedDate
        };
    }

    public async Task<TicketDto> AssignAsync(int orgId, int id, AssignTicketDto dto)
    {
        var ticket = await GetOwnedTicketAsync(orgId, id);

        ticket.AssignedToUserId = dto.UserId;
        if (ticket.Status == "Open")
            ticket.Status = "InProgress";
        ticket.UpdatedDate = DateTime.UtcNow;

        _repo.Update(ticket);
        await _repo.SaveChangesAsync();

        var refreshed = await _repo.GetByIdAsync(ticket.Id) ?? ticket;
        return MapToDto(refreshed);
    }

    public async Task<TicketDto> UpdateStatusAsync(int orgId, int id, UpdateTicketStatusDto dto)
    {
        if (!ValidStatuses.Contains(dto.Status))
            throw new InvalidOperationException($"Invalid status '{dto.Status}'.");

        var ticket = await GetOwnedTicketAsync(orgId, id);
        ticket.Status = dto.Status;
        if (dto.Status is "Resolved" or "Closed")
            ticket.ResolvedAt = DateTime.UtcNow;
        ticket.UpdatedDate = DateTime.UtcNow;

        _repo.Update(ticket);
        await _repo.SaveChangesAsync();
        return MapToDto(ticket);
    }

    public async Task<HelpDeskSummaryDto> GetSummaryAsync(int orgId)
    {
        var tickets = await _repo.GetAllByOrgAsync(orgId, null, null);
        var now = DateTime.UtcNow;

        return new HelpDeskSummaryDto
        {
            Open        = tickets.Count(t => t.Status == "Open"),
            InProgress  = tickets.Count(t => t.Status == "InProgress"),
            Resolved    = tickets.Count(t => t.Status == "Resolved"),
            Closed      = tickets.Count(t => t.Status == "Closed"),
            BreachedSla = tickets.Count(t => t.Status != "Resolved" && t.Status != "Closed"
                                              && t.SlaDueAt != null && t.SlaDueAt < now)
        };
    }

    private async Task<HelpDeskTicket> GetOwnedTicketAsync(int orgId, int id)
    {
        var ticket = await _repo.GetByIdAsync(id);
        if (ticket == null || ticket.OrganizationId != orgId)
            throw new KeyNotFoundException($"Ticket {id} not found");
        return ticket;
    }

    private static void EnsureCanAccess(HelpDeskTicket ticket, int? callerEmployeeId, bool canViewAll)
    {
        if (canViewAll) return;
        if (callerEmployeeId.HasValue && ticket.RaisedByEmployeeId == callerEmployeeId.Value) return;
        throw new UnauthorizedAccessException("You do not have access to this ticket.");
    }

    private static TicketDto MapToDto(HelpDeskTicket t)
    {
        var dto = new TicketDto();
        CopyTicketFields(dto, t);
        return dto;
    }

    private static TicketDetailDto MapToDetailDto(HelpDeskTicket t)
    {
        var dto = new TicketDetailDto();
        CopyTicketFields(dto, t);
        dto.Comments = t.Comments
            .OrderBy(c => c.CreatedDate)
            .Select(c => new TicketCommentDto
            {
                Id           = c.Id,
                AuthorUserId = c.AuthorUserId,
                AuthorName   = c.AuthorName,
                Comment      = c.Comment,
                CreatedDate  = c.CreatedDate
            })
            .ToList();
        return dto;
    }

    private static void CopyTicketFields(TicketDto dto, HelpDeskTicket t)
    {
        dto.Id                 = t.Id;
        dto.TicketNumber       = t.TicketNumber;
        dto.Category           = t.Category;
        dto.Priority           = t.Priority;
        dto.Subject            = t.Subject;
        dto.Description        = t.Description;
        dto.Status             = t.Status;
        dto.RaisedByEmployeeId = t.RaisedByEmployeeId;
        dto.RaisedByName       = t.RaisedByEmployee?.FullName ?? string.Empty;
        dto.AssignedToUserId   = t.AssignedToUserId;
        dto.AssignedToName     = t.AssignedToUser?.Name;
        dto.SlaDueAt           = t.SlaDueAt;
        dto.ResolvedAt         = t.ResolvedAt;
        dto.CommentCount       = t.Comments.Count;
        dto.CreatedDate        = t.CreatedDate;
    }
}
