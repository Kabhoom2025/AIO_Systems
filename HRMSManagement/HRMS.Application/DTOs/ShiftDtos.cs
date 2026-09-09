namespace HRMS.Application.DTOs;

public class ShiftDto
{
    public int      Id            { get; set; }
    public string   Name          { get; set; } = string.Empty;
    public string   Code          { get; set; } = string.Empty;
    public TimeOnly StartTime     { get; set; }
    public TimeOnly EndTime       { get; set; }
    public int      BreakMinutes  { get; set; }
    public int      GraceMinutes  { get; set; }
    public bool     IsNightShift  { get; set; }
    public string   WeeklyOffDays { get; set; } = string.Empty;
    public bool     IsActive      { get; set; }
}

public class CreateShiftDto
{
    public string   Name          { get; set; } = string.Empty;
    public string   Code          { get; set; } = string.Empty;
    public TimeOnly StartTime     { get; set; }
    public TimeOnly EndTime       { get; set; }
    public int      BreakMinutes  { get; set; } = 60;
    public int      GraceMinutes  { get; set; } = 10;
    public bool     IsNightShift  { get; set; }
    public string   WeeklyOffDays { get; set; } = "Saturday,Sunday";
}

public class UpdateShiftDto
{
    public string   Name          { get; set; } = string.Empty;
    public string   Code          { get; set; } = string.Empty;
    public TimeOnly StartTime     { get; set; }
    public TimeOnly EndTime       { get; set; }
    public int      BreakMinutes  { get; set; } = 60;
    public int      GraceMinutes  { get; set; } = 10;
    public bool     IsNightShift  { get; set; }
    public string   WeeklyOffDays { get; set; } = "Saturday,Sunday";
    public bool     IsActive      { get; set; }
}
