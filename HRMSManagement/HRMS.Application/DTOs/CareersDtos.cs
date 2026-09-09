namespace HRMS.Application.DTOs;

public class PublicOrgDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class PublicJobOpeningDto
{
    public int      Id                 { get; set; }
    public string   Title              { get; set; } = string.Empty;
    public string   DepartmentName     { get; set; } = string.Empty;
    public string?  Location           { get; set; }
    public string   EmploymentType     { get; set; } = string.Empty;
    public decimal? MinExperienceYears { get; set; }
    public decimal? MaxExperienceYears { get; set; }
    public int      Vacancies          { get; set; }
    public DateOnly PostedDate         { get; set; }
}

public class PublicJobOpeningDetailDto : PublicJobOpeningDto
{
    public string?  Description     { get; set; }
    public string   DesignationTitle { get; set; } = string.Empty;
    public decimal? SalaryRangeFrom { get; set; }
    public decimal? SalaryRangeTo   { get; set; }
}

public class ApplyToJobDto
{
    public string   Name                 { get; set; } = string.Empty;
    public string   Email                { get; set; } = string.Empty;
    public string?  Phone                { get; set; }
    public string?  CurrentCompany       { get; set; }
    public decimal? TotalExperienceYears { get; set; }
    public decimal? ExpectedSalary       { get; set; }
    public string?  ResumeUrl            { get; set; }
    /// <summary>Where the candidate found the posting — LinkedIn | Naukri | CompanyWebsite | Referral | Other.</summary>
    public string?  Source               { get; set; }
    public string?  CoverNote            { get; set; }
}

public class ApplyResultDto
{
    public int     CandidateId   { get; set; }
    public string  TrackingToken { get; set; } = string.Empty;
    public string  Message       { get; set; } = string.Empty;
}

public class TrackingStageDto
{
    public string  Stage    { get; set; } = string.Empty;
    public bool    IsDone   { get; set; }
    public bool    IsCurrent { get; set; }
}

public class TrackingStatusDto
{
    public string   CandidateName   { get; set; } = string.Empty;
    public string   JobTitle        { get; set; } = string.Empty;
    public string   CompanyName     { get; set; } = string.Empty;
    public string   CurrentStage    { get; set; } = string.Empty;
    public bool     IsRejected      { get; set; }
    public bool     IsHired         { get; set; }
    public int      ProgressPercent { get; set; }
    public DateTime AppliedDate     { get; set; }
    public List<TrackingStageDto>   Stages   { get; set; } = new();
    public List<StageHistoryDto>    Timeline { get; set; } = new();
}
