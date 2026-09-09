using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    // Attendance
    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceByDateAsync(DateTime date);
    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceByUserAsync(int userId, int month, int year);
    Task<AttendanceRecord?> GetAttendanceAsync(int userId, DateTime date);
    Task AddAttendanceAsync(AttendanceRecord record);
    Task SaveChangesAsync();

    // Salary
    Task<IReadOnlyList<EmployeeSalary>> GetAllSalaryConfigsAsync();
    Task<EmployeeSalary?> GetSalaryConfigAsync(int userId);
    Task AddSalaryConfigAsync(EmployeeSalary config);
    Task<IReadOnlyList<SalaryPayment>> GetPaymentsAsync(int? userId = null);
    Task AddPaymentAsync(SalaryPayment payment);
    Task<SalaryPayment?> GetPaymentByIdAsync(int id);

    // Shifts
    Task<IReadOnlyList<ShiftDefinition>> GetShiftDefinitionsAsync();
    Task<ShiftDefinition?> GetShiftDefinitionByIdAsync(int id);
    Task AddShiftDefinitionAsync(ShiftDefinition shift);
    Task DeleteShiftDefinitionAsync(ShiftDefinition shift);
    Task<IReadOnlyList<ShiftAssignment>> GetShiftAssignmentsAsync(DateTime start, DateTime end);
    Task<ShiftAssignment?> GetShiftAssignmentByIdAsync(int id);
    Task AddShiftAssignmentAsync(ShiftAssignment assignment);
    Task DeleteShiftAssignmentAsync(ShiftAssignment assignment);

    // Performance
    Task<IReadOnlyList<PerformanceReview>> GetReviewsAsync(int? userId = null);
    Task<PerformanceReview?> GetReviewByIdAsync(int id);
    Task AddReviewAsync(PerformanceReview review);
    Task DeleteReviewAsync(PerformanceReview review);
}
