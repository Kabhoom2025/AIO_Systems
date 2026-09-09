using System.Reflection;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly HrmsDbContext _ctx;

    public ReportService(HrmsDbContext ctx) => _ctx = ctx;

    public async Task<List<HeadcountReportRow>> GetHeadcountReportAsync(int orgId)
    {
        var employees = await _ctx.Employees
            .Include(e => e.Department)
            .Include(e => e.Branch)
            .Where(e => e.OrganizationId == orgId)
            .ToListAsync();

        return employees
            .GroupBy(e => new
            {
                Department = e.Department?.Name ?? "Unassigned",
                Branch     = e.Branch?.Name ?? "Unassigned"
            })
            .Select(g => new HeadcountReportRow
            {
                DepartmentName = g.Key.Department,
                BranchName     = g.Key.Branch,
                ActiveCount    = g.Count(e => e.Status == "Active"),
                OnNoticeCount  = g.Count(e => e.Status == "OnNotice"),
                ExitedCount    = g.Count(e => e.Status is "Resigned" or "Terminated" or "Retired")
            })
            .OrderBy(r => r.DepartmentName).ThenBy(r => r.BranchName)
            .ToList();
    }

    public async Task<List<AttendanceMonthlyReportRow>> GetAttendanceMonthlyReportAsync(int orgId, int year, int month)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);

        var employees = await _ctx.Employees
            .Where(e => e.OrganizationId == orgId)
            .ToListAsync();

        var attendance = await _ctx.AttendanceRecords
            .Where(a => a.OrganizationId == orgId && a.Date >= monthStart && a.Date <= monthEnd)
            .ToListAsync();

        var leaveRequests = await _ctx.LeaveRequests
            .Where(l => l.OrganizationId == orgId && l.Status == "Approved"
                     && l.StartDate <= monthEnd && l.EndDate >= monthStart)
            .ToListAsync();

        var attendanceByEmployee = attendance.ToLookup(a => a.EmployeeId);
        var leavesByEmployee     = leaveRequests.ToLookup(l => l.EmployeeId);

        var rows = new List<AttendanceMonthlyReportRow>();
        foreach (var emp in employees)
        {
            var empAttendance = attendanceByEmployee[emp.Id].ToList();
            var presentDays = empAttendance.Sum(a => a.Status switch
            {
                "Present" => 1m,
                "HalfDay" => 0.5m,
                _         => 0m
            });
            var lateCount = empAttendance.Count(a => a.LateMinutes > 0);
            var overtimeHours = Math.Round(empAttendance.Sum(a => a.OvertimeMinutes) / 60m, 1);

            var leaveDays = 0m;
            foreach (var l in leavesByEmployee[emp.Id])
            {
                var overlapStart = l.StartDate > monthStart ? l.StartDate : monthStart;
                var overlapEnd   = l.EndDate < monthEnd ? l.EndDate : monthEnd;
                if (overlapStart > overlapEnd) continue;

                var totalSpanDays = l.EndDate.DayNumber - l.StartDate.DayNumber + 1;
                var overlapSpanDays = overlapEnd.DayNumber - overlapStart.DayNumber + 1;
                leaveDays += totalSpanDays > 0 ? l.Days * overlapSpanDays / totalSpanDays : 0;
            }

            if (empAttendance.Count == 0 && leaveDays == 0) continue;

            rows.Add(new AttendanceMonthlyReportRow
            {
                EmployeeCode  = emp.EmployeeCode,
                EmployeeName  = emp.FullName,
                PresentDays   = presentDays,
                LeaveDays     = Math.Round(leaveDays, 1),
                LateCount     = lateCount,
                OvertimeHours = overtimeHours
            });
        }

        return rows.OrderBy(r => r.EmployeeCode).ToList();
    }

    public async Task<List<LeaveReportRow>> GetLeaveReportAsync(int orgId, int year)
    {
        var balances = await _ctx.LeaveBalances
            .Include(b => b.Employee)
            .Include(b => b.LeaveType)
            .Where(b => b.OrganizationId == orgId && b.Year == year)
            .ToListAsync();

        return balances
            .Select(b => new LeaveReportRow
            {
                EmployeeCode  = b.Employee.EmployeeCode,
                EmployeeName  = b.Employee.FullName,
                LeaveTypeName = b.LeaveType.Name,
                Allocated     = b.Allocated,
                Used          = b.Used,
                Available     = b.Allocated + b.CarriedForward - b.Used
            })
            .OrderBy(r => r.EmployeeCode).ThenBy(r => r.LeaveTypeName)
            .ToList();
    }

    public async Task<List<PayrollSummaryReportRow>> GetPayrollSummaryReportAsync(int orgId, int year)
    {
        var runs = await _ctx.PayrollRuns
            .Include(r => r.Payslips)
            .Where(r => r.OrganizationId == orgId && r.Year == year)
            .OrderBy(r => r.Month)
            .ToListAsync();

        return runs
            .Select(r => new PayrollSummaryReportRow
            {
                Year            = r.Year,
                Month           = r.Month,
                Status          = r.Status,
                EmployeeCount   = r.Payslips.Count,
                TotalGross      = r.TotalGross,
                TotalDeductions = r.TotalDeductions,
                TotalNet        = r.TotalNet
            })
            .ToList();
    }

    public async Task<List<RecruitmentPipelineReportRow>> GetRecruitmentPipelineReportAsync(int orgId)
    {
        var openings = await _ctx.JobOpenings
            .Include(o => o.Department)
            .Include(o => o.Candidates)
            .Where(o => o.OrganizationId == orgId)
            .ToListAsync();

        return openings
            .Select(o => new RecruitmentPipelineReportRow
            {
                Title           = o.Title,
                DepartmentName  = o.Department?.Name ?? "Unassigned",
                Status          = o.Status,
                Vacancies       = o.Vacancies,
                TotalCandidates = o.Candidates.Count,
                Applied         = o.Candidates.Count(c => c.Stage == "Applied"),
                Screening       = o.Candidates.Count(c => c.Stage == "Screening"),
                Interview       = o.Candidates.Count(c => c.Stage == "Interview"),
                Offered         = o.Candidates.Count(c => c.Stage == "Offered"),
                Hired           = o.Candidates.Count(c => c.Stage == "Hired"),
                Rejected        = o.Candidates.Count(c => c.Stage == "Rejected")
            })
            .OrderBy(r => r.Title)
            .ToList();
    }

    public async Task<List<AssetReportRow>> GetAssetsReportAsync(int orgId)
    {
        var assets = await _ctx.Assets
            .Include(a => a.Allocations)
                .ThenInclude(al => al.Employee)
            .Where(a => a.OrganizationId == orgId)
            .ToListAsync();

        return assets
            .Select(a =>
            {
                var openAllocation = a.Allocations
                    .Where(al => al.ReturnedDate == null)
                    .OrderByDescending(al => al.AllocatedDate)
                    .FirstOrDefault();

                return new AssetReportRow
                {
                    AssetTag      = a.AssetTag,
                    Name          = a.Name,
                    Category      = a.Category,
                    Status        = a.Status,
                    Condition     = a.Condition,
                    CurrentHolder = openAllocation?.Employee.FullName ?? string.Empty,
                    PurchaseCost  = a.PurchaseCost,
                    WarrantyUntil = a.WarrantyUntil
                };
            })
            .OrderBy(r => r.AssetTag)
            .ToList();
    }

    /// <summary>Small generic CSV writer used by ReportController to serve format=csv responses.</summary>
    public static class CsvBuilder
    {
        public static string Build<T>(IEnumerable<T> rows)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", props.Select(p => Escape(p.Name))));
            foreach (var row in rows)
            {
                var values = props.Select(p => Escape(FormatValue(p.GetValue(row))));
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        private static string FormatValue(object? value) => value switch
        {
            null           => string.Empty,
            DateOnly d     => d.ToString("yyyy-MM-dd"),
            DateTime dt    => dt.ToString("yyyy-MM-dd HH:mm"),
            decimal dec    => dec.ToString("0.##"),
            _              => value.ToString() ?? string.Empty
        };

        private static string Escape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
