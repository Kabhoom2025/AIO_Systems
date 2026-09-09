using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public class HrmsDbContext : DbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options) : base(options) { }

    // Organization structure
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Branch>       Branches      => Set<Branch>();
    public DbSet<Department>   Departments   => Set<Department>();
    public DbSet<Designation>  Designations  => Set<Designation>();
    public DbSet<JobGrade>     JobGrades     => Set<JobGrade>();
    public DbSet<Shift>        Shifts        => Set<Shift>();
    public DbSet<Holiday>      Holidays      => Set<Holiday>();

    // Security / RBAC
    public DbSet<Role>               Roles               => Set<Role>();
    public DbSet<Permission>         Permissions         => Set<Permission>();
    public DbSet<RolePermission>     RolePermissions     => Set<RolePermission>();
    public DbSet<User>               Users               => Set<User>();
    public DbSet<RefreshToken>       RefreshTokens       => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<LoginHistory>       LoginHistories      => Set<LoginHistory>();

    // Employees
    public DbSet<Employee>               Employees               => Set<Employee>();
    public DbSet<EmployeeDocument>       EmployeeDocuments       => Set<EmployeeDocument>();
    public DbSet<EmployeeEducation>      EmployeeEducations      => Set<EmployeeEducation>();
    public DbSet<EmployeeExperience>     EmployeeExperiences     => Set<EmployeeExperience>();
    public DbSet<EmployeeFamilyMember>   EmployeeFamilyMembers   => Set<EmployeeFamilyMember>();
    public DbSet<EmployeeLifecycleEvent> EmployeeLifecycleEvents => Set<EmployeeLifecycleEvent>();

    // Attendance & leave
    public DbSet<AttendanceRecord>         AttendanceRecords         => Set<AttendanceRecord>();
    public DbSet<AttendanceRegularization> AttendanceRegularizations => Set<AttendanceRegularization>();
    public DbSet<LeaveType>                LeaveTypes                => Set<LeaveType>();
    public DbSet<LeaveBalance>             LeaveBalances             => Set<LeaveBalance>();
    public DbSet<LeaveRequest>             LeaveRequests             => Set<LeaveRequest>();

    // Payroll
    public DbSet<SalaryComponent>    SalaryComponents    => Set<SalaryComponent>();
    public DbSet<EmployeeSalary>     EmployeeSalaries    => Set<EmployeeSalary>();
    public DbSet<EmployeeSalaryItem> EmployeeSalaryItems => Set<EmployeeSalaryItem>();
    public DbSet<PayrollRun>         PayrollRuns         => Set<PayrollRun>();
    public DbSet<Payslip>            Payslips            => Set<Payslip>();
    public DbSet<PayslipItem>        PayslipItems        => Set<PayslipItem>();

    // Recruitment & performance
    public DbSet<JobOpening>              JobOpenings          => Set<JobOpening>();
    public DbSet<Candidate>               Candidates           => Set<Candidate>();
    public DbSet<CandidateStageHistory>   CandidateStageHistories => Set<CandidateStageHistory>();
    public DbSet<Interview>               Interviews           => Set<Interview>();
    public DbSet<PerformanceGoal>   PerformanceGoals   => Set<PerformanceGoal>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();

    // Assets, expenses, help desk
    public DbSet<Asset>           Assets           => Set<Asset>();
    public DbSet<AssetAllocation> AssetAllocations => Set<AssetAllocation>();
    public DbSet<ExpenseClaim>    ExpenseClaims    => Set<ExpenseClaim>();
    public DbSet<HelpDeskTicket>  HelpDeskTickets  => Set<HelpDeskTicket>();
    public DbSet<TicketComment>   TicketComments   => Set<TicketComment>();

    // Platform
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog>     AuditLogs     => Set<AuditLog>();

    // Workflow engine
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowVersion>    WorkflowVersions    => Set<WorkflowVersion>();
    public DbSet<WorkflowExecution>  WorkflowExecutions  => Set<WorkflowExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmsDbContext).Assembly);
    }
}
