using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IExpenseService
{
    Task<ExpenseClaimDto> CreateAsync(int orgId, int employeeId, CreateExpenseClaimDto dto);
    Task<List<ExpenseClaimDto>> GetMyClaimsAsync(int orgId, int employeeId);
    Task<List<ExpenseClaimDto>> GetAllAsync(int orgId, string? status);
    Task<ExpenseClaimDto> GetByIdAsync(int orgId, int id);
    Task<ExpenseClaimDto> ApproveAsync(int orgId, int id, int reviewerUserId, ReviewExpenseDto dto);
    Task<ExpenseClaimDto> RejectAsync(int orgId, int id, int reviewerUserId, ReviewExpenseDto dto);
    Task<ExpenseClaimDto> ReimburseAsync(int orgId, int id);
    Task DeleteAsync(int orgId, int id, int employeeId);
    Task<ExpenseSummaryDto> GetSummaryAsync(int orgId);
}
