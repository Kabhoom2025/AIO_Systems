using System.Text;
using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;

    public EmployeeService(IEmployeeRepository repo) => _repo = repo;

    public async Task<PagedResult<EmployeeListDto>> GetPagedAsync(
        int orgId, int page, int pageSize, string? search, int? departmentId, int? branchId, string? status)
    {
        var (items, total) = await _repo.GetPagedAsync(orgId, search, departmentId, branchId, status, page, pageSize);
        return new PagedResult<EmployeeListDto>
        {
            Items      = items.Select(MapToListDto).ToList(),
            TotalCount = total,
            Page       = page < 1 ? 1 : page,
            PageSize   = pageSize < 1 || pageSize > 200 ? 20 : pageSize
        };
    }

    public async Task<List<EmployeeLookupDto>> GetLookupAsync(int orgId)
    {
        var employees = await _repo.GetLookupAsync(orgId);
        return employees.Select(MapToLookupDto).ToList();
    }

    public async Task<EmployeeDetailDto> GetByIdAsync(int orgId, int id)
    {
        var employee = await GetOwnedAsync(orgId, id);
        return MapToDetailDto(employee);
    }

    public async Task<EmployeeDetailDto> GetMyProfileAsync(int orgId, int employeeId)
    {
        var employee = await _repo.GetByIdAsync(orgId, employeeId)
            ?? throw new KeyNotFoundException("No employee profile linked to this account.");
        return MapToDetailDto(employee);
    }

    public async Task<EmployeeDetailDto> CreateAsync(int orgId, CreateEmployeeDto dto)
    {
        if (await _repo.WorkEmailExistsAsync(orgId, dto.WorkEmail, null))
            throw new InvalidOperationException($"An employee with work email '{dto.WorkEmail}' already exists.");

        var nextNumber = await _repo.GetNextCodeNumberAsync(orgId);

        var employee = new Employee
        {
            OrganizationId    = orgId,
            EmployeeCode      = $"EMP-{nextNumber:D4}",
            BranchId          = dto.BranchId,
            DepartmentId      = dto.DepartmentId,
            DesignationId     = dto.DesignationId,
            ShiftId           = dto.ShiftId,
            ManagerId         = dto.ManagerId,
            FirstName         = dto.FirstName,
            LastName          = dto.LastName,
            Gender            = dto.Gender,
            DateOfBirth       = dto.DateOfBirth,
            MaritalStatus     = dto.MaritalStatus,
            BloodGroup        = dto.BloodGroup,
            Nationality       = dto.Nationality,
            WorkEmail         = dto.WorkEmail,
            PersonalEmail     = dto.PersonalEmail,
            Phone             = dto.Phone,
            CurrentAddress    = dto.CurrentAddress,
            PermanentAddress  = dto.PermanentAddress,
            PhotoUrl          = dto.PhotoUrl,
            JoiningDate       = dto.JoiningDate,
            ConfirmationDate  = dto.ConfirmationDate,
            EmploymentType    = dto.EmploymentType,
            Status            = "Active",
            NationalIdNumber  = dto.NationalIdNumber,
            TaxIdNumber       = dto.TaxIdNumber,
            PfNumber          = dto.PfNumber,
            EsiNumber         = dto.EsiNumber,
            PassportNumber    = dto.PassportNumber,
            PassportExpiry    = dto.PassportExpiry,
            BankName          = dto.BankName,
            BankAccountNumber = dto.BankAccountNumber,
            BankIfscCode      = dto.BankIfscCode,
            Skills            = dto.Skills,
            Languages         = dto.Languages,
            Notes             = dto.Notes
        };

        _repo.Add(employee);
        await _repo.SaveChangesAsync();

        // Reload with navigation properties so we can read the designation title for the lifecycle remark.
        var created = await _repo.GetByIdAsync(orgId, employee.Id)
            ?? throw new InvalidOperationException("Failed to load newly created employee.");

        _repo.AddLifecycleEvent(new EmployeeLifecycleEvent
        {
            EmployeeId = created.Id,
            EventType  = "Joining",
            EventDate  = created.JoiningDate,
            Remarks    = $"Joined as {created.Designation.Title}"
        });
        await _repo.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(orgId, employee.Id)
            ?? throw new InvalidOperationException("Failed to load newly created employee.");
        return MapToDetailDto(result);
    }

    public async Task<EmployeeDetailDto> UpdateAsync(int orgId, int id, UpdateEmployeeDto dto)
    {
        var employee = await GetOwnedAsync(orgId, id);

        if (await _repo.WorkEmailExistsAsync(orgId, dto.WorkEmail, id))
            throw new InvalidOperationException($"An employee with work email '{dto.WorkEmail}' already exists.");

        employee.BranchId          = dto.BranchId;
        employee.DepartmentId      = dto.DepartmentId;
        employee.DesignationId     = dto.DesignationId;
        employee.ShiftId           = dto.ShiftId;
        employee.ManagerId         = dto.ManagerId;
        employee.FirstName         = dto.FirstName;
        employee.LastName          = dto.LastName;
        employee.Gender            = dto.Gender;
        employee.DateOfBirth       = dto.DateOfBirth;
        employee.MaritalStatus     = dto.MaritalStatus;
        employee.BloodGroup        = dto.BloodGroup;
        employee.Nationality       = dto.Nationality;
        employee.WorkEmail         = dto.WorkEmail;
        employee.PersonalEmail     = dto.PersonalEmail;
        employee.Phone             = dto.Phone;
        employee.CurrentAddress    = dto.CurrentAddress;
        employee.PermanentAddress  = dto.PermanentAddress;
        employee.PhotoUrl          = dto.PhotoUrl;
        employee.JoiningDate       = dto.JoiningDate;
        employee.ConfirmationDate  = dto.ConfirmationDate;
        employee.ExitDate          = dto.ExitDate;
        employee.EmploymentType    = dto.EmploymentType;
        employee.Status            = dto.Status;
        employee.NationalIdNumber  = dto.NationalIdNumber;
        employee.TaxIdNumber       = dto.TaxIdNumber;
        employee.PfNumber          = dto.PfNumber;
        employee.EsiNumber         = dto.EsiNumber;
        employee.PassportNumber    = dto.PassportNumber;
        employee.PassportExpiry    = dto.PassportExpiry;
        employee.BankName          = dto.BankName;
        employee.BankAccountNumber = dto.BankAccountNumber;
        employee.BankIfscCode      = dto.BankIfscCode;
        employee.Skills            = dto.Skills;
        employee.Languages         = dto.Languages;
        employee.Notes             = dto.Notes;
        employee.UpdatedDate       = DateTime.UtcNow;

        _repo.Update(employee);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetByIdAsync(orgId, id)
            ?? throw new InvalidOperationException("Failed to load updated employee.");
        return MapToDetailDto(updated);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var employee = await GetOwnedAsync(orgId, id);
        var exitDate = DateOnly.FromDateTime(DateTime.UtcNow);

        employee.Status      = "Terminated";
        employee.ExitDate    = exitDate;
        employee.UpdatedDate = DateTime.UtcNow;
        _repo.Update(employee);

        _repo.AddLifecycleEvent(new EmployeeLifecycleEvent
        {
            EmployeeId = employee.Id,
            EventType  = "Termination",
            EventDate  = exitDate,
            Remarks    = "Employee terminated"
        });

        await _repo.SaveChangesAsync();
    }

    public async Task<EmployeeDocumentDto> AddDocumentAsync(int orgId, int employeeId, CreateEmployeeDocumentDto dto)
    {
        var employee = await GetOwnedAsync(orgId, employeeId);
        var document = new EmployeeDocument
        {
            EmployeeId = employee.Id,
            Type       = dto.Type,
            Name       = dto.Name,
            FilePath   = dto.FilePath,
            ExpiryDate = dto.ExpiryDate,
            Notes      = dto.Notes
        };
        _repo.AddDocument(document);
        await _repo.SaveChangesAsync();
        return MapToDocumentDto(document);
    }

    public async Task RemoveDocumentAsync(int orgId, int employeeId, int docId)
    {
        await GetOwnedAsync(orgId, employeeId);
        var document = await _repo.GetDocumentAsync(employeeId, docId)
            ?? throw new KeyNotFoundException($"Document {docId} not found");
        _repo.RemoveDocument(document);
        await _repo.SaveChangesAsync();
    }

    public async Task<EmployeeEducationDto> AddEducationAsync(int orgId, int employeeId, CreateEmployeeEducationDto dto)
    {
        var employee = await GetOwnedAsync(orgId, employeeId);
        var education = new EmployeeEducation
        {
            EmployeeId   = employee.Id,
            Degree       = dto.Degree,
            Institution  = dto.Institution,
            FieldOfStudy = dto.FieldOfStudy,
            StartYear    = dto.StartYear,
            EndYear      = dto.EndYear,
            Grade        = dto.Grade
        };
        _repo.AddEducation(education);
        await _repo.SaveChangesAsync();
        return MapToEducationDto(education);
    }

    public async Task RemoveEducationAsync(int orgId, int employeeId, int eduId)
    {
        await GetOwnedAsync(orgId, employeeId);
        var education = await _repo.GetEducationAsync(employeeId, eduId)
            ?? throw new KeyNotFoundException($"Education {eduId} not found");
        _repo.RemoveEducation(education);
        await _repo.SaveChangesAsync();
    }

    public async Task<EmployeeExperienceDto> AddExperienceAsync(int orgId, int employeeId, CreateEmployeeExperienceDto dto)
    {
        var employee = await GetOwnedAsync(orgId, employeeId);
        var experience = new EmployeeExperience
        {
            EmployeeId  = employee.Id,
            Company     = dto.Company,
            Title       = dto.Title,
            StartDate   = dto.StartDate,
            EndDate     = dto.EndDate,
            Description = dto.Description
        };
        _repo.AddExperience(experience);
        await _repo.SaveChangesAsync();
        return MapToExperienceDto(experience);
    }

    public async Task RemoveExperienceAsync(int orgId, int employeeId, int expId)
    {
        await GetOwnedAsync(orgId, employeeId);
        var experience = await _repo.GetExperienceAsync(employeeId, expId)
            ?? throw new KeyNotFoundException($"Experience {expId} not found");
        _repo.RemoveExperience(experience);
        await _repo.SaveChangesAsync();
    }

    public async Task<EmployeeFamilyMemberDto> AddFamilyMemberAsync(int orgId, int employeeId, CreateEmployeeFamilyMemberDto dto)
    {
        var employee = await GetOwnedAsync(orgId, employeeId);
        var member = new EmployeeFamilyMember
        {
            EmployeeId         = employee.Id,
            Name               = dto.Name,
            Relationship       = dto.Relationship,
            Phone              = dto.Phone,
            DateOfBirth        = dto.DateOfBirth,
            IsEmergencyContact = dto.IsEmergencyContact
        };
        _repo.AddFamilyMember(member);
        await _repo.SaveChangesAsync();
        return MapToFamilyMemberDto(member);
    }

    public async Task RemoveFamilyMemberAsync(int orgId, int employeeId, int memberId)
    {
        await GetOwnedAsync(orgId, employeeId);
        var member = await _repo.GetFamilyMemberAsync(employeeId, memberId)
            ?? throw new KeyNotFoundException($"Family member {memberId} not found");
        _repo.RemoveFamilyMember(member);
        await _repo.SaveChangesAsync();
    }

    public async Task<LifecycleEventDto> AddLifecycleEventAsync(int orgId, int employeeId, CreateLifecycleEventDto dto)
    {
        var employee = await GetOwnedAsync(orgId, employeeId);
        var lifecycleEvent = new EmployeeLifecycleEvent
        {
            EmployeeId = employee.Id,
            EventType  = dto.EventType,
            EventDate  = dto.EventDate,
            FromValue  = dto.FromValue,
            ToValue    = dto.ToValue,
            Remarks    = dto.Remarks
        };
        _repo.AddLifecycleEvent(lifecycleEvent);
        await _repo.SaveChangesAsync();
        return MapToLifecycleDto(lifecycleEvent);
    }

    public async Task<List<LifecycleEventDto>> GetLifecycleEventsAsync(int orgId, int employeeId)
    {
        await GetOwnedAsync(orgId, employeeId);
        var events = await _repo.GetLifecycleEventsAsync(employeeId);
        return events.Select(MapToLifecycleDto).ToList();
    }

    public async Task<List<EmployeeLookupDto>> GetTeamAsync(int orgId, int managerId)
    {
        var reports = await _repo.GetDirectReportsAsync(orgId, managerId);
        return reports.Select(MapToLookupDto).ToList();
    }

    public async Task<string> ExportCsvAsync(int orgId)
    {
        var employees = await _repo.GetAllForExportAsync(orgId);
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Email,Phone,Branch,Department,Designation,Type,Status,JoiningDate");

        foreach (var e in employees)
        {
            sb.AppendLine(string.Join(",",
                CsvEscape(e.EmployeeCode),
                CsvEscape(e.FullName),
                CsvEscape(e.WorkEmail),
                CsvEscape(e.Phone ?? string.Empty),
                CsvEscape(e.Branch?.Name ?? string.Empty),
                CsvEscape(e.Department?.Name ?? string.Empty),
                CsvEscape(e.Designation?.Title ?? string.Empty),
                CsvEscape(e.EmploymentType),
                CsvEscape(e.Status),
                e.JoiningDate.ToString("yyyy-MM-dd")));
        }

        return sb.ToString();
    }

    private async Task<Employee> GetOwnedAsync(int orgId, int id)
    {
        var employee = await _repo.GetByIdAsync(orgId, id);
        if (employee == null)
            throw new KeyNotFoundException($"Employee {id} not found");
        return employee;
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static EmployeeListDto MapToListDto(Employee e) => new()
    {
        Id               = e.Id,
        EmployeeCode     = e.EmployeeCode,
        FullName         = e.FullName,
        WorkEmail        = e.WorkEmail,
        Phone            = e.Phone,
        PhotoUrl         = e.PhotoUrl,
        BranchName       = e.Branch?.Name ?? string.Empty,
        DepartmentName   = e.Department?.Name ?? string.Empty,
        DesignationTitle = e.Designation?.Title ?? string.Empty,
        ManagerName      = e.Manager?.FullName,
        EmploymentType   = e.EmploymentType,
        Status           = e.Status,
        JoiningDate      = e.JoiningDate
    };

    private static EmployeeLookupDto MapToLookupDto(Employee e) => new()
    {
        Id               = e.Id,
        EmployeeCode     = e.EmployeeCode,
        FullName         = e.FullName,
        DesignationTitle = e.Designation?.Title ?? string.Empty
    };

    private static EmployeeDocumentDto MapToDocumentDto(EmployeeDocument d) => new()
    {
        Id         = d.Id,
        Type       = d.Type,
        Name       = d.Name,
        FilePath   = d.FilePath,
        ExpiryDate = d.ExpiryDate,
        Notes      = d.Notes
    };

    private static EmployeeEducationDto MapToEducationDto(EmployeeEducation ed) => new()
    {
        Id           = ed.Id,
        Degree       = ed.Degree,
        Institution  = ed.Institution,
        FieldOfStudy = ed.FieldOfStudy,
        StartYear    = ed.StartYear,
        EndYear      = ed.EndYear,
        Grade        = ed.Grade
    };

    private static EmployeeExperienceDto MapToExperienceDto(EmployeeExperience ex) => new()
    {
        Id          = ex.Id,
        Company     = ex.Company,
        Title       = ex.Title,
        StartDate   = ex.StartDate,
        EndDate     = ex.EndDate,
        Description = ex.Description
    };

    private static EmployeeFamilyMemberDto MapToFamilyMemberDto(EmployeeFamilyMember f) => new()
    {
        Id                 = f.Id,
        Name               = f.Name,
        Relationship       = f.Relationship,
        Phone              = f.Phone,
        DateOfBirth        = f.DateOfBirth,
        IsEmergencyContact = f.IsEmergencyContact
    };

    private static LifecycleEventDto MapToLifecycleDto(EmployeeLifecycleEvent l) => new()
    {
        Id        = l.Id,
        EventType = l.EventType,
        EventDate = l.EventDate,
        FromValue = l.FromValue,
        ToValue   = l.ToValue,
        Remarks   = l.Remarks
    };

    private static EmployeeDetailDto MapToDetailDto(Employee e) => new()
    {
        Id                = e.Id,
        EmployeeCode      = e.EmployeeCode,
        BranchId          = e.BranchId,
        BranchName        = e.Branch?.Name ?? string.Empty,
        DepartmentId      = e.DepartmentId,
        DepartmentName    = e.Department?.Name ?? string.Empty,
        DesignationId     = e.DesignationId,
        DesignationTitle  = e.Designation?.Title ?? string.Empty,
        ShiftId           = e.ShiftId,
        ShiftName         = e.Shift?.Name,
        ManagerId         = e.ManagerId,
        ManagerName       = e.Manager?.FullName,
        FirstName         = e.FirstName,
        LastName          = e.LastName,
        FullName          = e.FullName,
        Gender            = e.Gender,
        DateOfBirth       = e.DateOfBirth,
        MaritalStatus     = e.MaritalStatus,
        BloodGroup        = e.BloodGroup,
        Nationality       = e.Nationality,
        WorkEmail         = e.WorkEmail,
        PersonalEmail     = e.PersonalEmail,
        Phone             = e.Phone,
        CurrentAddress    = e.CurrentAddress,
        PermanentAddress  = e.PermanentAddress,
        PhotoUrl          = e.PhotoUrl,
        JoiningDate       = e.JoiningDate,
        ConfirmationDate  = e.ConfirmationDate,
        ExitDate          = e.ExitDate,
        EmploymentType    = e.EmploymentType,
        Status            = e.Status,
        NationalIdNumber  = e.NationalIdNumber,
        TaxIdNumber       = e.TaxIdNumber,
        PfNumber          = e.PfNumber,
        EsiNumber         = e.EsiNumber,
        PassportNumber    = e.PassportNumber,
        PassportExpiry    = e.PassportExpiry,
        BankName          = e.BankName,
        BankAccountNumber = e.BankAccountNumber,
        BankIfscCode      = e.BankIfscCode,
        Skills            = e.Skills,
        Languages         = e.Languages,
        Notes             = e.Notes,
        Documents         = e.Documents.Select(MapToDocumentDto).ToList(),
        Educations        = e.Educations.Select(MapToEducationDto).ToList(),
        Experiences       = e.Experiences.Select(MapToExperienceDto).ToList(),
        FamilyMembers     = e.FamilyMembers.Select(MapToFamilyMemberDto).ToList(),
        LifecycleEvents   = e.LifecycleEvents
            .OrderByDescending(l => l.EventDate)
            .ThenByDescending(l => l.Id)
            .Select(MapToLifecycleDto)
            .ToList()
    };
}
