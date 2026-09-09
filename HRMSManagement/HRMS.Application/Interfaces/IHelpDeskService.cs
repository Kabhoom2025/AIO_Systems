using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IHelpDeskService
{
    Task<TicketDto> CreateTicketAsync(int orgId, int employeeId, CreateTicketDto dto);
    Task<List<TicketDto>> GetMyTicketsAsync(int orgId, int employeeId);
    Task<List<TicketDto>> GetAllAsync(int orgId, string? status, string? category);
    Task<TicketDetailDto> GetByIdAsync(int orgId, int id, int? callerEmployeeId, bool canViewAll);
    Task<TicketCommentDto> AddCommentAsync(int orgId, int id, int? userId, string userName,
        int? callerEmployeeId, bool canViewAll, AddCommentDto dto);
    Task<TicketDto> AssignAsync(int orgId, int id, AssignTicketDto dto);
    Task<TicketDto> UpdateStatusAsync(int orgId, int id, UpdateTicketStatusDto dto);
    Task<HelpDeskSummaryDto> GetSummaryAsync(int orgId);
}
