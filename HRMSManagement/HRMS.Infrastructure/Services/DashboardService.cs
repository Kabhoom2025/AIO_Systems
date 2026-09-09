using System.Globalization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly HrmsDbContext _ctx;

    public DashboardService(HrmsDbContext ctx) => _ctx = ctx;

    public async Task<DashboardSummaryDto> GetSummaryAsync(int orgId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalEmployees = await _ctx.Employees
            .Where(e => e.OrganizationId == orgId && e.Status == "Active")
            .CountAsync();

        var presentToday = await _ctx.AttendanceRecords
            .Where(a => a.OrganizationId == orgId && a.Date == today && a.CheckInAt != null)
            .CountAsync();

        var onLeaveToday = await _ctx.LeaveRequests
            .Where(l => l.OrganizationId == orgId && l.Status == "Approved"
                     && l.StartDate <= today && l.EndDate >= today)
            .CountAsync();

        var pendingLeaveApprovals = await _ctx.LeaveRequests
            .Where(l => l.OrganizationId == orgId && l.Status == "Pending")
            .CountAsync();

        var pendingRegularizations = await _ctx.AttendanceRegularizations
            .Where(r => r.OrganizationId == orgId && r.Status == "Pending")
            .CountAsync();

        var pendingExpenseClaims = await _ctx.ExpenseClaims
            .Where(x => x.OrganizationId == orgId && x.Status == "Pending")
            .CountAsync();

        var openJobOpenings = await _ctx.JobOpenings
            .Where(j => j.OrganizationId == orgId && j.Status == "Open")
            .CountAsync();

        var openTickets = await _ctx.HelpDeskTickets
            .Where(t => t.OrganizationId == orgId && (t.Status == "Open" || t.Status == "InProgress"))
            .CountAsync();

        var upcomingHolidays = await _ctx.Holidays
            .Where(h => h.OrganizationId == orgId && h.Date >= today)
            .OrderBy(h => h.Date)
            .Take(5)
            .Select(h => new HolidaySummaryDto { Name = h.Name, Date = h.Date })
            .ToListAsync();

        var activeEmployees = await _ctx.Employees
            .Include(e => e.Department)
            .Where(e => e.OrganizationId == orgId && e.Status == "Active")
            .ToListAsync();

        var headcountByDepartment = activeEmployees
            .GroupBy(e => e.Department?.Name ?? "Unassigned")
            .Select(g => new NameValueDto { Name = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToList();

        var employmentTypeSplit = activeEmployees
            .GroupBy(e => e.EmploymentType)
            .Select(g => new NameValueDto { Name = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToList();

        var genderSplit = activeEmployees
            .GroupBy(e => e.Gender)
            .Select(g => new NameValueDto { Name = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToList();

        var attendanceTrend = await BuildAttendanceTrendAsync(orgId, today);

        var recentJoinersRaw = await _ctx.Employees
            .Include(e => e.Designation)
            .Where(e => e.OrganizationId == orgId)
            .OrderByDescending(e => e.JoiningDate)
            .Take(5)
            .ToListAsync();

        var recentJoiners = recentJoinersRaw
            .Select(e => new RecentJoinerDto
            {
                EmployeeCode     = e.EmployeeCode,
                FullName         = e.FullName,
                DesignationTitle = e.Designation?.Title ?? string.Empty,
                JoiningDate      = e.JoiningDate
            })
            .ToList();

        var birthdaysThisMonth = await _ctx.Employees
            .Where(e => e.OrganizationId == orgId && e.Status == "Active" && e.DateOfBirth != null
                     && e.DateOfBirth.Value.Month == today.Month)
            .ToListAsync();

        var birthdays = birthdaysThisMonth
            .OrderBy(e => e.DateOfBirth!.Value.Day)
            .Select(e => new BirthdayDto
            {
                FullName    = e.FullName,
                DateOfBirth = e.DateOfBirth!.Value.ToString("dd MMM", CultureInfo.InvariantCulture)
            })
            .ToList();

        return new DashboardSummaryDto
        {
            TotalEmployees         = totalEmployees,
            PresentToday           = presentToday,
            OnLeaveToday           = onLeaveToday,
            PendingLeaveApprovals  = pendingLeaveApprovals,
            PendingRegularizations = pendingRegularizations,
            PendingExpenseClaims   = pendingExpenseClaims,
            OpenJobOpenings        = openJobOpenings,
            OpenTickets            = openTickets,
            UpcomingHolidays       = upcomingHolidays,
            HeadcountByDepartment  = headcountByDepartment,
            EmploymentTypeSplit    = employmentTypeSplit,
            GenderSplit            = genderSplit,
            AttendanceTrend        = attendanceTrend,
            RecentJoiners          = recentJoiners,
            BirthdaysThisMonth     = birthdays
        };
    }

    private async Task<List<DateCountDto>> BuildAttendanceTrendAsync(int orgId, DateOnly today)
    {
        var startDate = today.AddDays(-13);

        var attendanceInRange = await _ctx.AttendanceRecords
            .Where(a => a.OrganizationId == orgId && a.Date >= startDate && a.Date <= today
                     && (a.Status == "Present" || a.Status == "HalfDay"))
            .Select(a => a.Date)
            .ToListAsync();

        var leavesInRange = await _ctx.LeaveRequests
            .Where(l => l.OrganizationId == orgId && l.Status == "Approved"
                     && l.StartDate <= today && l.EndDate >= startDate)
            .Select(l => new { l.StartDate, l.EndDate })
            .ToListAsync();

        var presentByDate = attendanceInRange
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        var trend = new List<DateCountDto>();
        for (var d = startDate; d <= today; d = d.AddDays(1))
        {
            var present = presentByDate.TryGetValue(d, out var c) ? c : 0;
            var onLeave = leavesInRange.Count(l => l.StartDate <= d && l.EndDate >= d);
            trend.Add(new DateCountDto { Date = d, Present = present, OnLeave = onLeave });
        }

        return trend;
    }

    public async Task<MyDashboardDto> GetMyDashboardAsync(int orgId, int? employeeId)
    {
        if (employeeId is null)
            return new MyDashboardDto { HasEmployeeProfile = false };

        var empId = employeeId.Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todayRecord = await _ctx.AttendanceRecords
            .Where(a => a.OrganizationId == orgId && a.EmployeeId == empId && a.Date == today)
            .FirstOrDefaultAsync();

        var todayAttendance = todayRecord is null
            ? null
            : new TodayAttendanceDto
            {
                CheckInAt  = todayRecord.CheckInAt,
                CheckOutAt = todayRecord.CheckOutAt,
                Status     = todayRecord.Status
            };

        var leaveBalances = await _ctx.LeaveBalances
            .Include(b => b.LeaveType)
            .Where(b => b.OrganizationId == orgId && b.EmployeeId == empId && b.Year == today.Year)
            .Select(b => new LeaveBalanceSummaryDto
            {
                LeaveTypeName = b.LeaveType.Name,
                Available     = b.Allocated + b.CarriedForward - b.Used
            })
            .ToListAsync();

        var pendingRequests = new PendingRequestsSummaryDto
        {
            PendingLeaveCount = await _ctx.LeaveRequests
                .Where(l => l.OrganizationId == orgId && l.EmployeeId == empId && l.Status == "Pending")
                .CountAsync(),
            PendingRegularizationCount = await _ctx.AttendanceRegularizations
                .Where(r => r.OrganizationId == orgId && r.EmployeeId == empId && r.Status == "Pending")
                .CountAsync(),
            PendingExpenseCount = await _ctx.ExpenseClaims
                .Where(x => x.OrganizationId == orgId && x.EmployeeId == empId && x.Status == "Pending")
                .CountAsync()
        };

        var goalsInProgress = await _ctx.PerformanceGoals
            .Where(g => g.OrganizationId == orgId && g.EmployeeId == empId && g.Status == "InProgress")
            .CountAsync();

        var latestPayslip = await _ctx.Payslips
            .Include(p => p.PayrollRun)
            .Where(p => p.EmployeeId == empId && p.PayrollRun.OrganizationId == orgId)
            .OrderByDescending(p => p.PayrollRun.Year)
            .ThenByDescending(p => p.PayrollRun.Month)
            .Select(p => new LatestPayslipDto
            {
                Year   = p.PayrollRun.Year,
                Month  = p.PayrollRun.Month,
                NetPay = p.NetPay
            })
            .FirstOrDefaultAsync();

        var myOpenTickets = await _ctx.HelpDeskTickets
            .Where(t => t.OrganizationId == orgId && t.RaisedByEmployeeId == empId
                     && (t.Status == "Open" || t.Status == "InProgress"))
            .CountAsync();

        var upcomingHolidays = await _ctx.Holidays
            .Where(h => h.OrganizationId == orgId && h.Date >= today)
            .OrderBy(h => h.Date)
            .Take(3)
            .Select(h => new HolidaySummaryDto { Name = h.Name, Date = h.Date })
            .ToListAsync();

        return new MyDashboardDto
        {
            HasEmployeeProfile = true,
            TodayAttendance    = todayAttendance,
            LeaveBalances      = leaveBalances,
            PendingRequests    = pendingRequests,
            GoalsInProgress    = goalsInProgress,
            LatestPayslip      = latestPayslip,
            MyOpenTickets      = myOpenTickets,
            UpcomingHolidays   = upcomingHolidays
        };
    }
}
