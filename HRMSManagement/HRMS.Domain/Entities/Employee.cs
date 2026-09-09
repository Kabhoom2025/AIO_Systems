namespace HRMS.Domain.Entities;

public class Employee : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int     BranchId       { get; set; }
    public int     DepartmentId   { get; set; }
    public int     DesignationId  { get; set; }
    public int?    ShiftId        { get; set; }
    public int?    ManagerId      { get; set; } // reporting manager (employee)

    public string  EmployeeCode   { get; set; } = string.Empty; // e.g. EMP-0001
    public string  FirstName      { get; set; } = string.Empty;
    public string  LastName       { get; set; } = string.Empty;
    public string  Gender         { get; set; } = "Male";       // Male | Female | Other
    public DateOnly? DateOfBirth  { get; set; }
    public string? MaritalStatus  { get; set; }                 // Single | Married | …
    public string? BloodGroup     { get; set; }
    public string? Nationality    { get; set; }

    public string  WorkEmail      { get; set; } = string.Empty;
    public string? PersonalEmail  { get; set; }
    public string? Phone          { get; set; }
    public string? CurrentAddress   { get; set; }
    public string? PermanentAddress { get; set; }
    public string? PhotoUrl       { get; set; }

    public DateOnly  JoiningDate      { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ExitDate         { get; set; }
    public string    EmploymentType   { get; set; } = "FullTime"; // FullTime | PartTime | Contract | Intern
    public string    Status           { get; set; } = "Active";   // Active | OnNotice | Resigned | Terminated | Retired

    // Statutory & bank
    public string? NationalIdNumber { get; set; } // Aadhaar / SSN …
    public string? TaxIdNumber      { get; set; } // PAN …
    public string? PfNumber         { get; set; }
    public string? EsiNumber        { get; set; }
    public string? PassportNumber   { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public string? BankName         { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIfscCode     { get; set; }

    public string? Skills    { get; set; } // comma-separated
    public string? Languages { get; set; } // comma-separated
    public string? Notes     { get; set; }

    public Organization Organization { get; set; } = null!;
    public Branch       Branch       { get; set; } = null!;
    public Department   Department   { get; set; } = null!;
    public Designation  Designation  { get; set; } = null!;
    public Shift?       Shift        { get; set; }
    public Employee?    Manager      { get; set; }

    public ICollection<Employee>               DirectReports   { get; set; } = new List<Employee>();
    public ICollection<EmployeeDocument>       Documents       { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeEducation>      Educations      { get; set; } = new List<EmployeeEducation>();
    public ICollection<EmployeeExperience>     Experiences     { get; set; } = new List<EmployeeExperience>();
    public ICollection<EmployeeFamilyMember>   FamilyMembers   { get; set; } = new List<EmployeeFamilyMember>();
    public ICollection<EmployeeLifecycleEvent> LifecycleEvents { get; set; } = new List<EmployeeLifecycleEvent>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class EmployeeDocument : BaseEntity
{
    public int      EmployeeId { get; set; }
    public string   Type       { get; set; } = string.Empty; // Passport | Visa | DrivingLicense | Contract | Certificate | Other
    public string   Name       { get; set; } = string.Empty;
    public string?  FilePath   { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string?  Notes      { get; set; }

    public Employee Employee { get; set; } = null!;
}

public class EmployeeEducation : BaseEntity
{
    public int     EmployeeId   { get; set; }
    public string  Degree       { get; set; } = string.Empty;
    public string  Institution  { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public int?    StartYear    { get; set; }
    public int?    EndYear      { get; set; }
    public string? Grade        { get; set; }

    public Employee Employee { get; set; } = null!;
}

public class EmployeeExperience : BaseEntity
{
    public int      EmployeeId  { get; set; }
    public string   Company     { get; set; } = string.Empty;
    public string   Title       { get; set; } = string.Empty;
    public DateOnly? StartDate  { get; set; }
    public DateOnly? EndDate    { get; set; }
    public string?  Description { get; set; }

    public Employee Employee { get; set; } = null!;
}

public class EmployeeFamilyMember : BaseEntity
{
    public int     EmployeeId         { get; set; }
    public string  Name               { get; set; } = string.Empty;
    public string  Relationship       { get; set; } = string.Empty; // Spouse | Father | Mother | Child | …
    public string? Phone              { get; set; }
    public DateOnly? DateOfBirth      { get; set; }
    public bool    IsEmergencyContact { get; set; }

    public Employee Employee { get; set; } = null!;
}

/// <summary>Timeline entries: Joining, Confirmation, Promotion, Transfer, Increment, Resignation, Termination, Retirement.</summary>
public class EmployeeLifecycleEvent : BaseEntity
{
    public int      EmployeeId { get; set; }
    public string   EventType  { get; set; } = string.Empty;
    public DateOnly EventDate  { get; set; }
    public string?  FromValue  { get; set; } // e.g. old designation / branch / salary
    public string?  ToValue    { get; set; }
    public string?  Remarks    { get; set; }

    public Employee Employee { get; set; } = null!;
}
