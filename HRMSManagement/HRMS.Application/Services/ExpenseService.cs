using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repo;

    public ExpenseService(IExpenseRepository repo) => _repo = repo;

    public async Task<ExpenseClaimDto> CreateAsync(int orgId, int employeeId, CreateExpenseClaimDto dto)
    {
        if (dto.Amount <= 0)
            throw new InvalidOperationException("Amount must be greater than zero.");

        var claim = new ExpenseClaim
        {
            OrganizationId = orgId,
            EmployeeId     = employeeId,
            Category       = dto.Category,
            ClaimDate      = dto.ClaimDate,
            Amount         = dto.Amount,
            Currency       = string.IsNullOrWhiteSpace(dto.Currency) ? "INR" : dto.Currency,
            Description    = dto.Description,
            ReceiptUrl     = dto.ReceiptUrl,
            Status         = "Pending"
        };
        _repo.Add(claim);
        await _repo.SaveChangesAsync();
        return MapToDto(claim);
    }

    public async Task<List<ExpenseClaimDto>> GetMyClaimsAsync(int orgId, int employeeId)
    {
        var claims = await _repo.GetByEmployeeAsync(orgId, employeeId);
        return claims.Select(MapToDto).ToList();
    }

    public async Task<List<ExpenseClaimDto>> GetAllAsync(int orgId, string? status)
    {
        var claims = await _repo.GetAllByOrgAsync(orgId, status);
        return claims.Select(MapToDto).ToList();
    }

    public async Task<ExpenseClaimDto> GetByIdAsync(int orgId, int id)
    {
        var claim = await GetOwnedClaimAsync(orgId, id);
        return MapToDto(claim);
    }

    public async Task<ExpenseClaimDto> ApproveAsync(int orgId, int id, int reviewerUserId, ReviewExpenseDto dto)
    {
        var claim = await GetOwnedClaimAsync(orgId, id);
        if (claim.Status != "Pending")
            throw new InvalidOperationException("Only pending claims can be approved.");

        claim.Status           = "Approved";
        claim.ReviewedByUserId = reviewerUserId;
        claim.ReviewedAt       = DateTime.UtcNow;
        claim.ReviewNotes      = dto.Notes;

        _repo.Update(claim);
        await _repo.SaveChangesAsync();
        return MapToDto(claim);
    }

    public async Task<ExpenseClaimDto> RejectAsync(int orgId, int id, int reviewerUserId, ReviewExpenseDto dto)
    {
        var claim = await GetOwnedClaimAsync(orgId, id);
        if (claim.Status != "Pending")
            throw new InvalidOperationException("Only pending claims can be rejected.");

        claim.Status           = "Rejected";
        claim.ReviewedByUserId = reviewerUserId;
        claim.ReviewedAt       = DateTime.UtcNow;
        claim.ReviewNotes      = dto.Notes;

        _repo.Update(claim);
        await _repo.SaveChangesAsync();
        return MapToDto(claim);
    }

    public async Task<ExpenseClaimDto> ReimburseAsync(int orgId, int id)
    {
        var claim = await GetOwnedClaimAsync(orgId, id);
        if (claim.Status != "Approved")
            throw new InvalidOperationException("Only approved claims can be reimbursed.");

        claim.Status       = "Reimbursed";
        claim.ReimbursedAt = DateTime.UtcNow;

        _repo.Update(claim);
        await _repo.SaveChangesAsync();
        return MapToDto(claim);
    }

    public async Task DeleteAsync(int orgId, int id, int employeeId)
    {
        var claim = await GetOwnedClaimAsync(orgId, id);
        if (claim.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only delete your own expense claims.");
        if (claim.Status != "Pending")
            throw new InvalidOperationException("Only pending claims can be deleted.");

        _repo.Remove(claim);
        await _repo.SaveChangesAsync();
    }

    public async Task<ExpenseSummaryDto> GetSummaryAsync(int orgId)
    {
        var claims = await _repo.GetAllByOrgAsync(orgId, null);
        return new ExpenseSummaryDto
        {
            Pending               = claims.Count(c => c.Status == "Pending"),
            Approved              = claims.Count(c => c.Status == "Approved"),
            Reimbursed            = claims.Count(c => c.Status == "Reimbursed"),
            Rejected              = claims.Count(c => c.Status == "Rejected"),
            TotalPendingAmount    = claims.Where(c => c.Status == "Pending").Sum(c => c.Amount),
            TotalReimbursedAmount = claims.Where(c => c.Status == "Reimbursed").Sum(c => c.Amount)
        };
    }

    private async Task<ExpenseClaim> GetOwnedClaimAsync(int orgId, int id)
    {
        var claim = await _repo.GetByIdAsync(id);
        if (claim == null || claim.OrganizationId != orgId)
            throw new KeyNotFoundException($"Expense claim {id} not found");
        return claim;
    }

    private static ExpenseClaimDto MapToDto(ExpenseClaim e) => new()
    {
        Id               = e.Id,
        EmployeeId       = e.EmployeeId,
        EmployeeName     = e.Employee?.FullName ?? string.Empty,
        EmployeeCode     = e.Employee?.EmployeeCode ?? string.Empty,
        Category         = e.Category,
        ClaimDate        = e.ClaimDate,
        Amount           = e.Amount,
        Currency         = e.Currency,
        Description      = e.Description,
        ReceiptUrl       = e.ReceiptUrl,
        Status           = e.Status,
        ReviewedByUserId = e.ReviewedByUserId,
        ReviewedByName   = e.ReviewedByUser?.Name,
        ReviewedAt       = e.ReviewedAt,
        ReviewNotes      = e.ReviewNotes,
        ReimbursedAt     = e.ReimbursedAt
    };
}
