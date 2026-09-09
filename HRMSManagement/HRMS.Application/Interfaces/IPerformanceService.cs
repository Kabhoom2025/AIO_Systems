using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IPerformanceService
{
    // Goals
    Task<List<PerformanceGoalDto>> GetGoalsByEmployeeAsync(int orgId, int employeeId);
    Task<PerformanceGoalDto> CreateGoalAsync(int orgId, CreatePerformanceGoalDto dto);
    Task<PerformanceGoalDto> UpdateGoalAsync(int orgId, int id, UpdatePerformanceGoalDto dto);
    Task<PerformanceGoalDto> UpdateGoalProgressAsync(int orgId, int id, int? callerEmployeeId, bool callerCanEdit, UpdateGoalProgressDto dto);
    Task DeleteGoalAsync(int orgId, int id);

    // Reviews
    Task<List<PerformanceReviewDto>> GetReviewsAsync(int orgId, string? period, string? status);
    Task<List<PerformanceReviewDto>> GetReviewsByEmployeeAsync(int orgId, int employeeId);
    Task<PerformanceReviewDto> GetReviewAsync(int orgId, int id);
    Task<PerformanceReviewDto> CreateReviewAsync(int orgId, CreatePerformanceReviewDto dto);
    Task<PerformanceReviewDto> SubmitSelfReviewAsync(int orgId, int id, int employeeId, SelfReviewDto dto);
    Task<PerformanceReviewDto> SubmitManagerReviewAsync(int orgId, int id, ManagerReviewDto dto);
    Task DeleteReviewAsync(int orgId, int id);
}
