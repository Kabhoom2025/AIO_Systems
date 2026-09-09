namespace HRMS.Application.DTOs;

public class NameValueDto
{
    public string Name  { get; set; } = string.Empty;
    public int    Value { get; set; }
}

public class DateCountDto
{
    public DateOnly Date      { get; set; }
    public int      Present   { get; set; }
    public int      OnLeave   { get; set; }
}

public class HolidaySummaryDto
{
    public string   Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}

public class RecentJoinerDto
{
    public string   EmployeeCode     { get; set; } = string.Empty;
    public string   FullName         { get; set; } = string.Empty;
    public string   DesignationTitle { get; set; } = string.Empty;
    public DateOnly JoiningDate      { get; set; }
}

public class BirthdayDto
{
    public string FullName    { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty; // formatted "dd MMM"
}

public class DashboardSummaryDto
{
    public int TotalEmployees        { get; set; }
    public int PresentToday          { get; set; }
    public int OnLeaveToday          { get; set; }
    public int PendingLeaveApprovals { get; set; }
    public int PendingRegularizations{ get; set; }
    public int PendingExpenseClaims  { get; set; }
    public int OpenJobOpenings       { get; set; }
    public int OpenTickets           { get; set; }

    public List<HolidaySummaryDto> UpcomingHolidays      { get; set; } = new();
    public List<NameValueDto>      HeadcountByDepartment  { get; set; } = new();
    public List<NameValueDto>      EmploymentTypeSplit    { get; set; } = new();
    public List<NameValueDto>      GenderSplit            { get; set; } = new();
    public List<DateCountDto>      AttendanceTrend        { get; set; } = new();
    public List<RecentJoinerDto>   RecentJoiners          { get; set; } = new();
    public List<BirthdayDto>       BirthdaysThisMonth     { get; set; } = new();
}

public class TodayAttendanceDto
{
    public DateTime? CheckInAt  { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string    Status     { get; set; } = string.Empty;
}

public class LeaveBalanceSummaryDto
{
    public string  LeaveTypeName { get; set; } = string.Empty;
    public decimal Available     { get; set; }
}

public class PendingRequestsSummaryDto
{
    public int PendingLeaveCount          { get; set; }
    public int PendingRegularizationCount { get; set; }
    public int PendingExpenseCount        { get; set; }
}

public class LatestPayslipDto
{
    public int     Year   { get; set; }
    public int     Month  { get; set; }
    public decimal NetPay { get; set; }
}

public class MyDashboardDto
{
    public bool HasEmployeeProfile { get; set; }

    public TodayAttendanceDto?     TodayAttendance { get; set; }
    public List<LeaveBalanceSummaryDto> LeaveBalances   { get; set; } = new();
    public PendingRequestsSummaryDto    PendingRequests { get; set; } = new();
    public int              GoalsInProgress { get; set; }
    public LatestPayslipDto? LatestPayslip  { get; set; }
    public int              MyOpenTickets   { get; set; }
    public List<HolidaySummaryDto> UpcomingHolidays { get; set; } = new();
}
