namespace HRMS.Application.DTOs;

public class HolidayDto
{
    public int      Id          { get; set; }
    public int?     BranchId    { get; set; }
    public string?  BranchName  { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public DateOnly Date        { get; set; }
    public string   Type        { get; set; } = "National";
    public string?  Description { get; set; }
}

public class CreateHolidayDto
{
    public int?     BranchId    { get; set; } // null = all branches
    public string   Name        { get; set; } = string.Empty;
    public DateOnly Date        { get; set; }
    public string   Type        { get; set; } = "National"; // National | Regional | Optional
    public string?  Description { get; set; }
}

public class UpdateHolidayDto
{
    public int?     BranchId    { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public DateOnly Date        { get; set; }
    public string   Type        { get; set; } = "National";
    public string?  Description { get; set; }
}
