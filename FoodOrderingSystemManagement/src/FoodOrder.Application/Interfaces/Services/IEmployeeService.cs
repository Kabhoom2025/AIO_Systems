using FoodOrder.Application.DTOs.Employee;
using FoodOrder.Application.DTOs.User;

namespace FoodOrder.Application.Interfaces.Services;

public interface IEmployeeService
{
    // User edit / delete
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);
    Task DeleteUserAsync(int id, int requestingUserId);

    // Attendance
    Task<IReadOnlyList<AttendanceDto>> GetAttendanceByDateAsync(DateTime date);
    Task<IReadOnlyList<AttendanceDto>> GetAttendanceByUserAsync(int userId, int month, int year);
    Task<AttendanceDto> UpsertAttendanceAsync(UpsertAttendanceRequest req);

    // Salary
    Task<IReadOnlyList<EmployeeSalaryDto>> GetSalaryConfigsAsync(int? organizationId);
    Task<EmployeeSalaryDto?> UpdateSalaryConfigAsync(int userId, UpdateSalaryConfigRequest req);
    Task<IReadOnlyList<SalaryPaymentDto>> GetPaymentsAsync(int? userId = null);
    Task<SalaryPaymentDto> CreatePaymentAsync(CreateSalaryPaymentRequest req);
    Task<SalaryPaymentDto?> MarkPaymentPaidAsync(int paymentId, int paidById);

    // Shifts
    Task<IReadOnlyList<ShiftDefinitionDto>> GetShiftDefinitionsAsync();
    Task<ShiftDefinitionDto> CreateShiftDefinitionAsync(CreateShiftDefinitionRequest req);
    Task DeleteShiftDefinitionAsync(int id);
    Task<IReadOnlyList<ShiftAssignmentDto>> GetShiftAssignmentsAsync(DateTime startDate, DateTime endDate);
    Task<ShiftAssignmentDto> CreateShiftAssignmentAsync(CreateShiftAssignmentRequest req);
    Task DeleteShiftAssignmentAsync(int id);

    // Performance
    Task<IReadOnlyList<PerformanceReviewDto>> GetReviewsAsync(int? userId = null);
    Task<PerformanceReviewDto> CreateReviewAsync(int reviewedById, CreatePerformanceReviewRequest req);
    Task DeleteReviewAsync(int id);
}
