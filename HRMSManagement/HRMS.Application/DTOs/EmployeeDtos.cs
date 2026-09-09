namespace HRMS.Application.DTOs;

public class EmployeeListDto
{
    public int       Id               { get; set; }
    public string    EmployeeCode     { get; set; } = string.Empty;
    public string    FullName         { get; set; } = string.Empty;
    public string    WorkEmail        { get; set; } = string.Empty;
    public string?   Phone            { get; set; }
    public string?   PhotoUrl         { get; set; }
    public string    BranchName       { get; set; } = string.Empty;
    public string    DepartmentName   { get; set; } = string.Empty;
    public string    DesignationTitle { get; set; } = string.Empty;
    public string?   ManagerName      { get; set; }
    public string    EmploymentType   { get; set; } = string.Empty;
    public string    Status           { get; set; } = string.Empty;
    public DateOnly  JoiningDate      { get; set; }
}

public class EmployeeDetailDto
{
    public int     Id               { get; set; }
    public string  EmployeeCode     { get; set; } = string.Empty;

    public int     BranchId         { get; set; }
    public string  BranchName       { get; set; } = string.Empty;
    public int     DepartmentId     { get; set; }
    public string  DepartmentName   { get; set; } = string.Empty;
    public int     DesignationId    { get; set; }
    public string  DesignationTitle { get; set; } = string.Empty;
    public int?    ShiftId          { get; set; }
    public string? ShiftName        { get; set; }
    public int?    ManagerId        { get; set; }
    public string? ManagerName      { get; set; }

    public string    FirstName        { get; set; } = string.Empty;
    public string    LastName         { get; set; } = string.Empty;
    public string    FullName         { get; set; } = string.Empty;
    public string    Gender           { get; set; } = string.Empty;
    public DateOnly? DateOfBirth      { get; set; }
    public string?   MaritalStatus    { get; set; }
    public string?   BloodGroup       { get; set; }
    public string?   Nationality      { get; set; }

    public string  WorkEmail          { get; set; } = string.Empty;
    public string? PersonalEmail      { get; set; }
    public string? Phone              { get; set; }
    public string? CurrentAddress     { get; set; }
    public string? PermanentAddress   { get; set; }
    public string? PhotoUrl           { get; set; }

    public DateOnly  JoiningDate      { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ExitDate         { get; set; }
    public string    EmploymentType   { get; set; } = string.Empty;
    public string    Status           { get; set; } = string.Empty;

    public string?   NationalIdNumber  { get; set; }
    public string?   TaxIdNumber       { get; set; }
    public string?   PfNumber          { get; set; }
    public string?   EsiNumber         { get; set; }
    public string?   PassportNumber    { get; set; }
    public DateOnly? PassportExpiry    { get; set; }
    public string?   BankName          { get; set; }
    public string?   BankAccountNumber { get; set; }
    public string?   BankIfscCode      { get; set; }

    public string? Skills    { get; set; }
    public string? Languages { get; set; }
    public string? Notes     { get; set; }

    public List<EmployeeDocumentDto>     Documents       { get; set; } = new();
    public List<EmployeeEducationDto>    Educations      { get; set; } = new();
    public List<EmployeeExperienceDto>   Experiences     { get; set; } = new();
    public List<EmployeeFamilyMemberDto> FamilyMembers   { get; set; } = new();
    public List<LifecycleEventDto>       LifecycleEvents { get; set; } = new();
}

public class CreateEmployeeDto
{
    public int  BranchId      { get; set; }
    public int  DepartmentId  { get; set; }
    public int  DesignationId { get; set; }
    public int? ShiftId       { get; set; }
    public int? ManagerId     { get; set; }

    public string    FirstName     { get; set; } = string.Empty;
    public string    LastName      { get; set; } = string.Empty;
    public string    Gender        { get; set; } = "Male";
    public DateOnly? DateOfBirth   { get; set; }
    public string?   MaritalStatus { get; set; }
    public string?   BloodGroup    { get; set; }
    public string?   Nationality   { get; set; }

    public string  WorkEmail        { get; set; } = string.Empty;
    public string? PersonalEmail    { get; set; }
    public string? Phone            { get; set; }
    public string? CurrentAddress   { get; set; }
    public string? PermanentAddress { get; set; }
    public string? PhotoUrl         { get; set; }

    public DateOnly  JoiningDate      { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public string    EmploymentType   { get; set; } = "FullTime";

    public string?   NationalIdNumber  { get; set; }
    public string?   TaxIdNumber       { get; set; }
    public string?   PfNumber          { get; set; }
    public string?   EsiNumber         { get; set; }
    public string?   PassportNumber    { get; set; }
    public DateOnly? PassportExpiry    { get; set; }
    public string?   BankName          { get; set; }
    public string?   BankAccountNumber { get; set; }
    public string?   BankIfscCode      { get; set; }

    public string? Skills    { get; set; }
    public string? Languages { get; set; }
    public string? Notes     { get; set; }
}

public class UpdateEmployeeDto
{
    public int  BranchId      { get; set; }
    public int  DepartmentId  { get; set; }
    public int  DesignationId { get; set; }
    public int? ShiftId       { get; set; }
    public int? ManagerId     { get; set; }

    public string    FirstName     { get; set; } = string.Empty;
    public string    LastName      { get; set; } = string.Empty;
    public string    Gender        { get; set; } = "Male";
    public DateOnly? DateOfBirth   { get; set; }
    public string?   MaritalStatus { get; set; }
    public string?   BloodGroup    { get; set; }
    public string?   Nationality   { get; set; }

    public string  WorkEmail        { get; set; } = string.Empty;
    public string? PersonalEmail    { get; set; }
    public string? Phone            { get; set; }
    public string? CurrentAddress   { get; set; }
    public string? PermanentAddress { get; set; }
    public string? PhotoUrl         { get; set; }

    public DateOnly  JoiningDate      { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ExitDate         { get; set; }
    public string    EmploymentType   { get; set; } = "FullTime";
    public string    Status           { get; set; } = "Active";

    public string?   NationalIdNumber  { get; set; }
    public string?   TaxIdNumber       { get; set; }
    public string?   PfNumber          { get; set; }
    public string?   EsiNumber         { get; set; }
    public string?   PassportNumber    { get; set; }
    public DateOnly? PassportExpiry    { get; set; }
    public string?   BankName          { get; set; }
    public string?   BankAccountNumber { get; set; }
    public string?   BankIfscCode      { get; set; }

    public string? Skills    { get; set; }
    public string? Languages { get; set; }
    public string? Notes     { get; set; }
}

public class EmployeeDocumentDto
{
    public int       Id         { get; set; }
    public string    Type       { get; set; } = string.Empty;
    public string    Name       { get; set; } = string.Empty;
    public string?   FilePath   { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string?   Notes      { get; set; }
}

public class CreateEmployeeDocumentDto
{
    public string    Type       { get; set; } = string.Empty;
    public string    Name       { get; set; } = string.Empty;
    public string?   FilePath   { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string?   Notes      { get; set; }
}

public class EmployeeEducationDto
{
    public int     Id           { get; set; }
    public string  Degree       { get; set; } = string.Empty;
    public string  Institution  { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public int?    StartYear    { get; set; }
    public int?    EndYear      { get; set; }
    public string? Grade        { get; set; }
}

public class CreateEmployeeEducationDto
{
    public string  Degree       { get; set; } = string.Empty;
    public string  Institution  { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public int?    StartYear    { get; set; }
    public int?    EndYear      { get; set; }
    public string? Grade        { get; set; }
}

public class EmployeeExperienceDto
{
    public int       Id          { get; set; }
    public string    Company     { get; set; } = string.Empty;
    public string    Title       { get; set; } = string.Empty;
    public DateOnly? StartDate   { get; set; }
    public DateOnly? EndDate     { get; set; }
    public string?   Description { get; set; }
}

public class CreateEmployeeExperienceDto
{
    public string    Company     { get; set; } = string.Empty;
    public string    Title       { get; set; } = string.Empty;
    public DateOnly? StartDate   { get; set; }
    public DateOnly? EndDate     { get; set; }
    public string?   Description { get; set; }
}

public class EmployeeFamilyMemberDto
{
    public int       Id                 { get; set; }
    public string    Name               { get; set; } = string.Empty;
    public string    Relationship       { get; set; } = string.Empty;
    public string?   Phone              { get; set; }
    public DateOnly? DateOfBirth        { get; set; }
    public bool      IsEmergencyContact { get; set; }
}

public class CreateEmployeeFamilyMemberDto
{
    public string    Name               { get; set; } = string.Empty;
    public string    Relationship       { get; set; } = string.Empty;
    public string?   Phone              { get; set; }
    public DateOnly? DateOfBirth        { get; set; }
    public bool      IsEmergencyContact { get; set; }
}

public class LifecycleEventDto
{
    public int       Id        { get; set; }
    public string    EventType { get; set; } = string.Empty;
    public DateOnly  EventDate { get; set; }
    public string?   FromValue { get; set; }
    public string?   ToValue   { get; set; }
    public string?   Remarks   { get; set; }
}

public class CreateLifecycleEventDto
{
    public string    EventType { get; set; } = string.Empty;
    public DateOnly  EventDate { get; set; }
    public string?   FromValue { get; set; }
    public string?   ToValue   { get; set; }
    public string?   Remarks   { get; set; }
}

public class EmployeeLookupDto
{
    public int    Id               { get; set; }
    public string EmployeeCode     { get; set; } = string.Empty;
    public string FullName         { get; set; } = string.Empty;
    public string DesignationTitle { get; set; } = string.Empty;
}
