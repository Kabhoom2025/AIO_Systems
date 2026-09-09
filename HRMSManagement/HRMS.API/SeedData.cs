using HRMS.Application.Common;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Authentication;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>One-time demo data seeded on first run against an empty HRMSDB (dev only).</summary>
public static class SeedData
{
    public static async Task SeedAsync(HrmsDbContext db)
    {
        if (await db.Organizations.AnyAsync()) return; // already seeded

        // Permission catalog (system-wide, not org-scoped)
        var permissions = PermissionCatalog.All
            .Select(p => new Permission { Key = p.Key, Module = p.Module, Description = p.Description })
            .ToList();
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        Permission[] ByKeys(params string[] keys) => permissions.Where(p => keys.Contains(p.Key)).ToArray();
        var allPermissions = permissions.ToArray();

        var adminRole = new Role
        {
            Name = "Admin", IsSystemRole = true,
            RolePermissions = allPermissions.Select(p => new RolePermission { Permission = p }).ToList()
        };
        var hrManagerRole = new Role
        {
            Name = "HR Manager", IsSystemRole = true,
            RolePermissions = allPermissions
                .Where(p => p.Key != "settings.edit" && p.Key != "audit-logs.delete")
                .Select(p => new RolePermission { Permission = p }).ToList()
        };
        var managerRole = new Role
        {
            Name = "Manager", IsSystemRole = true,
            RolePermissions = ByKeys(
                    "employees.view", "attendance.view", "attendance.approve",
                    "leaves.view", "leaves.approve", "expenses.view", "expenses.approve",
                    "performance.view", "performance.edit", "performance.review",
                    "recruitment.view", "recruitment.create", "recruitment.edit",
                    "reports.view")
                .Select(p => new RolePermission { Permission = p }).ToList()
        };
        var employeeRole = new Role
        {
            Name = "Employee", IsSystemRole = true,
            RolePermissions = ByKeys("helpdesk.view").Select(p => new RolePermission { Permission = p }).ToList()
        };

        db.Roles.AddRange(adminRole, hrManagerRole, managerRole, employeeRole);

        var org = new Organization
        {
            Name = "Cloudleap Technologies", Code = "CLOUDLEAP",
            LegalName = "Cloudleap Technologies Pvt Ltd",
            Address = "Bagmane Tech Park, Bengaluru, Karnataka, India",
            Phone = "+91-9876500000", Email = "hr@cloudleap.com", Website = "https://cloudleap.com",
            TaxNumber = "29AACCT1234F1Z8", Timezone = "Asia/Kolkata", Currency = "INR", IsActive = true
        };

        var headOffice = new Branch
        {
            Organization = org, Name = "Bengaluru Head Office", Code = "BLR-HO",
            Address = "Bagmane Tech Park, Bengaluru", City = "Bengaluru", State = "Karnataka", Country = "India",
            Phone = "+91-9876500000", Email = "blr@cloudleap.com", Timezone = "Asia/Kolkata",
            IsHeadOffice = true, IsActive = true
        };
        var puneBranch = new Branch
        {
            Organization = org, Name = "Pune Office", Code = "PUN-01",
            Address = "Hinjewadi Phase 2, Pune", City = "Pune", State = "Maharashtra", Country = "India",
            Phone = "+91-9876500001", Email = "pune@cloudleap.com", Timezone = "Asia/Kolkata",
            IsHeadOffice = false, IsActive = true
        };

        var engineeringDept = new Department { Organization = org, Branch = headOffice, Name = "Engineering", Code = "ENG", Description = "Product engineering", IsActive = true };
        var hrDept          = new Department { Organization = org, Branch = headOffice, Name = "Human Resources", Code = "HR", Description = "People operations", IsActive = true };
        var salesDept       = new Department { Organization = org, Branch = puneBranch, Name = "Sales", Code = "SALES", Description = "Sales & business development", IsActive = true };
        var financeDept     = new Department { Organization = org, Branch = headOffice, Name = "Finance", Code = "FIN", Description = "Finance & accounts", IsActive = true };

        var grade1 = new JobGrade { Organization = org, Name = "G1", Level = 1, MinAnnualSalary = 400_000, MaxAnnualSalary = 700_000, IsActive = true };
        var grade2 = new JobGrade { Organization = org, Name = "G2", Level = 2, MinAnnualSalary = 700_000, MaxAnnualSalary = 1_200_000, IsActive = true };
        var grade3 = new JobGrade { Organization = org, Name = "G3", Level = 3, MinAnnualSalary = 1_200_000, MaxAnnualSalary = 2_200_000, IsActive = true };
        var grade4 = new JobGrade { Organization = org, Name = "G4", Level = 4, MinAnnualSalary = 2_200_000, MaxAnnualSalary = 4_000_000, IsActive = true };

        var desigCeo    = new Designation { Organization = org, Title = "Chief Executive Officer", Code = "CEO",       JobGrade = grade4, IsActive = true };
        var desigHrMgr  = new Designation { Organization = org, Title = "HR Manager",               Code = "HRM",      JobGrade = grade3, IsActive = true };
        var desigEngMgr = new Designation { Organization = org, Title = "Engineering Manager",       Code = "ENGMGR",  JobGrade = grade3, IsActive = true };
        var desigSrDev  = new Designation { Organization = org, Title = "Senior Software Engineer",  Code = "SSE",     JobGrade = grade2, IsActive = true };
        var desigDev    = new Designation { Organization = org, Title = "Software Engineer",         Code = "SE",      JobGrade = grade1, IsActive = true };
        var desigSales  = new Designation { Organization = org, Title = "Sales Executive",            Code = "SALESEX", JobGrade = grade1, IsActive = true };

        var generalShift = new Shift
        {
            Organization = org, Name = "General Shift", Code = "GEN",
            StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(18, 30),
            BreakMinutes = 60, GraceMinutes = 10, IsNightShift = false,
            WeeklyOffDays = "Saturday,Sunday", IsActive = true
        };

        db.Organizations.Add(org);
        db.Branches.AddRange(headOffice, puneBranch);
        db.Departments.AddRange(engineeringDept, hrDept, salesDept, financeDept);
        db.JobGrades.AddRange(grade1, grade2, grade3, grade4);
        db.Designations.AddRange(desigCeo, desigHrMgr, desigEngMgr, desigSrDev, desigDev, desigSales);
        db.Shifts.Add(generalShift);
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var ceoEmployee = new Employee
        {
            Organization = org, Branch = headOffice, Department = engineeringDept, Designation = desigCeo, Shift = generalShift,
            EmployeeCode = "EMP-0001", FirstName = "Arjun", LastName = "Mehta", Gender = "Male",
            DateOfBirth = new DateOnly(1982, 3, 14), MaritalStatus = "Married", BloodGroup = "O+", Nationality = "Indian",
            WorkEmail = "arjun.mehta@cloudleap.com", Phone = "+91-9900000001",
            CurrentAddress = "Indiranagar, Bengaluru", JoiningDate = today.AddYears(-6), ConfirmationDate = today.AddYears(-6).AddMonths(3),
            EmploymentType = "FullTime", Status = "Active",
            BankName = "HDFC Bank", BankAccountNumber = "50100123456789", BankIfscCode = "HDFC0000123"
        };

        var hrManagerEmployee = new Employee
        {
            Organization = org, Branch = headOffice, Department = hrDept, Designation = desigHrMgr, Shift = generalShift, Manager = ceoEmployee,
            EmployeeCode = "EMP-0002", FirstName = "Priya", LastName = "Nair", Gender = "Female",
            DateOfBirth = new DateOnly(1988, 7, 22), MaritalStatus = "Married", BloodGroup = "A+", Nationality = "Indian",
            WorkEmail = "priya.nair@cloudleap.com", Phone = "+91-9900000002",
            CurrentAddress = "Koramangala, Bengaluru", JoiningDate = today.AddYears(-4), ConfirmationDate = today.AddYears(-4).AddMonths(3),
            EmploymentType = "FullTime", Status = "Active",
            BankName = "ICICI Bank", BankAccountNumber = "60200123456789", BankIfscCode = "ICIC0000456"
        };

        var engManagerEmployee = new Employee
        {
            Organization = org, Branch = headOffice, Department = engineeringDept, Designation = desigEngMgr, Shift = generalShift, Manager = ceoEmployee,
            EmployeeCode = "EMP-0003", FirstName = "Rahul", LastName = "Verma", Gender = "Male",
            DateOfBirth = new DateOnly(1985, 11, 5), MaritalStatus = "Single", BloodGroup = "B+", Nationality = "Indian",
            WorkEmail = "rahul.verma@cloudleap.com", Phone = "+91-9900000003",
            CurrentAddress = "HSR Layout, Bengaluru", JoiningDate = today.AddYears(-5), ConfirmationDate = today.AddYears(-5).AddMonths(3),
            EmploymentType = "FullTime", Status = "Active",
            BankName = "Axis Bank", BankAccountNumber = "91100123456789", BankIfscCode = "UTIB0000789"
        };

        var srDevEmployee = new Employee
        {
            Organization = org, Branch = headOffice, Department = engineeringDept, Designation = desigSrDev, Shift = generalShift, Manager = engManagerEmployee,
            EmployeeCode = "EMP-0004", FirstName = "Sneha", LastName = "Iyer", Gender = "Female",
            DateOfBirth = new DateOnly(1993, 2, 18), MaritalStatus = "Single", BloodGroup = "AB+", Nationality = "Indian",
            WorkEmail = "sneha.iyer@cloudleap.com", Phone = "+91-9900000004",
            CurrentAddress = "Whitefield, Bengaluru", JoiningDate = today.AddYears(-3), ConfirmationDate = today.AddYears(-3).AddMonths(3),
            EmploymentType = "FullTime", Status = "Active",
            BankName = "SBI", BankAccountNumber = "31100123456789", BankIfscCode = "SBIN0000321"
        };

        var devEmployee = new Employee
        {
            Organization = org, Branch = headOffice, Department = engineeringDept, Designation = desigDev, Shift = generalShift, Manager = engManagerEmployee,
            EmployeeCode = "EMP-0005", FirstName = "Karthik", LastName = "Raj", Gender = "Male",
            DateOfBirth = new DateOnly(1998, 9, 30), MaritalStatus = "Single", BloodGroup = "O-", Nationality = "Indian",
            WorkEmail = "karthik.raj@cloudleap.com", Phone = "+91-9900000005",
            CurrentAddress = "Marathahalli, Bengaluru", JoiningDate = today.AddMonths(-8),
            EmploymentType = "FullTime", Status = "Active",
            BankName = "Kotak Mahindra Bank", BankAccountNumber = "71100123456789", BankIfscCode = "KKBK0000654"
        };

        var salesEmployee = new Employee
        {
            Organization = org, Branch = puneBranch, Department = salesDept, Designation = desigSales, Shift = generalShift, Manager = ceoEmployee,
            EmployeeCode = "EMP-0006", FirstName = "Ananya", LastName = "Deshmukh", Gender = "Female",
            DateOfBirth = new DateOnly(1995, 5, 12), MaritalStatus = "Single", BloodGroup = "B-", Nationality = "Indian",
            WorkEmail = "ananya.deshmukh@cloudleap.com", Phone = "+91-9900000006",
            CurrentAddress = "Hinjewadi, Pune", JoiningDate = today.AddYears(-2),
            EmploymentType = "FullTime", Status = "Active",
            BankName = "HDFC Bank", BankAccountNumber = "50100987654321", BankIfscCode = "HDFC0000987"
        };

        db.Employees.AddRange(ceoEmployee, hrManagerEmployee, engManagerEmployee, srDevEmployee, devEmployee, salesEmployee);
        await db.SaveChangesAsync();

        void AddJoining(Employee e, string title) => db.EmployeeLifecycleEvents.Add(new EmployeeLifecycleEvent
        {
            Employee = e, EventType = "Joining", EventDate = e.JoiningDate, Remarks = $"Joined as {title}"
        });
        AddJoining(ceoEmployee, desigCeo.Title);
        AddJoining(hrManagerEmployee, desigHrMgr.Title);
        AddJoining(engManagerEmployee, desigEngMgr.Title);
        AddJoining(srDevEmployee, desigSrDev.Title);
        AddJoining(devEmployee, desigDev.Title);
        AddJoining(salesEmployee, desigSales.Title);

        var adminUser = new User
        {
            Organization = org, Branch = headOffice, Employee = ceoEmployee, Name = "Arjun Mehta",
            Email = "admin@hrms.local", PasswordHash = PasswordHasher.Hash("Admin@123"), Role = adminRole, IsActive = true
        };
        var hrUser = new User
        {
            Organization = org, Branch = headOffice, Employee = hrManagerEmployee, Name = "Priya Nair",
            Email = "hr@hrms.local", PasswordHash = PasswordHasher.Hash("Hr@12345"), Role = hrManagerRole, IsActive = true
        };
        var managerUser = new User
        {
            Organization = org, Branch = headOffice, Employee = engManagerEmployee, Name = "Rahul Verma",
            Email = "manager@hrms.local", PasswordHash = PasswordHasher.Hash("Manager@123"), Role = managerRole, IsActive = true
        };
        var employeeUser = new User
        {
            Organization = org, Branch = headOffice, Employee = srDevEmployee, Name = "Sneha Iyer",
            Email = "employee@hrms.local", PasswordHash = PasswordHasher.Hash("Employee@123"), Role = employeeRole, IsActive = true
        };

        db.Users.AddRange(adminUser, hrUser, managerUser, employeeUser);

        // Leave types
        var casualLeave  = new LeaveType { Organization = org, Name = "Casual Leave", Code = "CL", IsPaid = true, AnnualQuota = 12, MaxCarryForward = 5, RequiresApproval = true, Color = "#4F46E5", IsActive = true };
        var sickLeave    = new LeaveType { Organization = org, Name = "Sick Leave",   Code = "SL", IsPaid = true, AnnualQuota = 10, MaxCarryForward = 0, RequiresApproval = true, Color = "#DC2626", IsActive = true };
        var earnedLeave  = new LeaveType { Organization = org, Name = "Earned Leave", Code = "EL", IsPaid = true, AnnualQuota = 18, MaxCarryForward = 10, RequiresApproval = true, Color = "#059669", IsActive = true };
        var unpaidLeave  = new LeaveType { Organization = org, Name = "Leave Without Pay", Code = "LWP", IsPaid = false, AnnualQuota = 0, MaxCarryForward = 0, RequiresApproval = true, Color = "#6B7280", IsActive = true };
        db.LeaveTypes.AddRange(casualLeave, sickLeave, earnedLeave, unpaidLeave);

        // Salary components
        var basic  = new SalaryComponent { Organization = org, Name = "Basic",              Code = "BASIC", Type = "Earning",   CalcType = "PercentOfBasic", DefaultValue = 100, IsTaxable = true,  IsStatutory = false, DisplayOrder = 1, IsActive = true };
        var hra    = new SalaryComponent { Organization = org, Name = "House Rent Allowance", Code = "HRA",   Type = "Earning",   CalcType = "PercentOfBasic", DefaultValue = 40,  IsTaxable = true,  IsStatutory = false, DisplayOrder = 2, IsActive = true };
        var special = new SalaryComponent { Organization = org, Name = "Special Allowance",  Code = "SPL",   Type = "Earning",   CalcType = "Fixed",          DefaultValue = 0,   IsTaxable = true,  IsStatutory = false, DisplayOrder = 3, IsActive = true };
        var pf     = new SalaryComponent { Organization = org, Name = "Provident Fund",      Code = "PF",    Type = "Deduction", CalcType = "PercentOfBasic", DefaultValue = 12,  IsTaxable = false, IsStatutory = true,  DisplayOrder = 4, IsActive = true };
        var pt     = new SalaryComponent { Organization = org, Name = "Professional Tax",    Code = "PT",    Type = "Deduction", CalcType = "Fixed",          DefaultValue = 200, IsTaxable = false, IsStatutory = true,  DisplayOrder = 5, IsActive = true };
        db.SalaryComponents.AddRange(basic, hra, special, pf, pt);

        // Holidays for current year
        var year = today.Year;
        db.Holidays.AddRange(
            new Holiday { Organization = org, Name = "New Year's Day", Date = new DateOnly(year, 1, 1), Type = "National" },
            new Holiday { Organization = org, Name = "Republic Day", Date = new DateOnly(year, 1, 26), Type = "National" },
            new Holiday { Organization = org, Name = "Independence Day", Date = new DateOnly(year, 8, 15), Type = "National" },
            new Holiday { Organization = org, Name = "Gandhi Jayanti", Date = new DateOnly(year, 10, 2), Type = "National" },
            new Holiday { Organization = org, Name = "Christmas", Date = new DateOnly(year, 12, 25), Type = "National" }
        );

        // Job openings + a sample candidate
        var opening = new JobOpening
        {
            Organization = org, Department = engineeringDept, Designation = desigDev,
            Title = "Software Engineer - Backend", Description = "Build and maintain HRMS backend services.",
            Vacancies = 2, Location = "Bengaluru", EmploymentType = "FullTime",
            MinExperienceYears = 1, MaxExperienceYears = 4, SalaryRangeFrom = 600_000, SalaryRangeTo = 1_000_000,
            Status = "Open", PostedDate = today.AddDays(-10)
        };
        db.JobOpenings.Add(opening);

        // Sample assets
        db.Assets.AddRange(
            new Asset { Organization = org, Branch = headOffice, AssetTag = "AST-0001", Name = "Dell Latitude 5440", Category = "Laptop", SerialNumber = "DL5440-001", PurchaseDate = today.AddYears(-1), PurchaseCost = 75000, WarrantyUntil = today.AddYears(2), Condition = "Good", Status = "Available" },
            new Asset { Organization = org, Branch = headOffice, AssetTag = "AST-0002", Name = "MacBook Pro 14", Category = "Laptop", SerialNumber = "MBP14-002", PurchaseDate = today.AddMonths(-6), PurchaseCost = 180000, WarrantyUntil = today.AddYears(3), Condition = "New", Status = "Available" }
        );

        await db.SaveChangesAsync();
    }
}
