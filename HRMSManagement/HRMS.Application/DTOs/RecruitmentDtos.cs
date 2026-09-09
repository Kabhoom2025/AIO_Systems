namespace HRMS.Application.DTOs;

public class JobOpeningDto
{
    public int      Id                 { get; set; }
    public int      DepartmentId       { get; set; }
    public string   DepartmentName     { get; set; } = string.Empty;
    public int      DesignationId      { get; set; }
    public string   DesignationTitle   { get; set; } = string.Empty;
    public string   Title              { get; set; } = string.Empty;
    public string?  Description        { get; set; }
    public int      Vacancies          { get; set; }
    public string?  Location           { get; set; }
    public string   EmploymentType     { get; set; } = string.Empty;
    public decimal? MinExperienceYears { get; set; }
    public decimal? MaxExperienceYears { get; set; }
    public decimal? SalaryRangeFrom    { get; set; }
    public decimal? SalaryRangeTo      { get; set; }
    public string   Status             { get; set; } = string.Empty;
    public DateOnly PostedDate         { get; set; }
    public DateOnly? ClosingDate       { get; set; }
    public int      CandidateCount     { get; set; }
}

public class CreateJobOpeningDto
{
    public int      DepartmentId       { get; set; }
    public int      DesignationId      { get; set; }
    public string   Title              { get; set; } = string.Empty;
    public string?  Description        { get; set; }
    public int      Vacancies          { get; set; } = 1;
    public string?  Location           { get; set; }
    public string   EmploymentType     { get; set; } = "FullTime";
    public decimal? MinExperienceYears { get; set; }
    public decimal? MaxExperienceYears { get; set; }
    public decimal? SalaryRangeFrom    { get; set; }
    public decimal? SalaryRangeTo      { get; set; }
    public DateOnly PostedDate         { get; set; }
    public DateOnly? ClosingDate       { get; set; }
}

public class UpdateJobOpeningDto
{
    public int      DepartmentId       { get; set; }
    public int      DesignationId      { get; set; }
    public string   Title              { get; set; } = string.Empty;
    public string?  Description        { get; set; }
    public int      Vacancies          { get; set; } = 1;
    public string?  Location           { get; set; }
    public string   EmploymentType     { get; set; } = "FullTime";
    public decimal? MinExperienceYears { get; set; }
    public decimal? MaxExperienceYears { get; set; }
    public decimal? SalaryRangeFrom    { get; set; }
    public decimal? SalaryRangeTo      { get; set; }
    public string   Status             { get; set; } = "Open"; // Open | OnHold | Closed
    public DateOnly PostedDate         { get; set; }
    public DateOnly? ClosingDate       { get; set; }
}

public class CandidateDto
{
    public int      Id                   { get; set; }
    public int      JobOpeningId         { get; set; }
    public string   JobOpeningTitle      { get; set; } = string.Empty;
    public string   Name                 { get; set; } = string.Empty;
    public string   Email                { get; set; } = string.Empty;
    public string?  Phone                { get; set; }
    public string?  ResumeUrl            { get; set; }
    public string?  CurrentCompany       { get; set; }
    public decimal? TotalExperienceYears { get; set; }
    public decimal? ExpectedSalary       { get; set; }
    public string?  Source               { get; set; }
    public string   Stage                { get; set; } = string.Empty;
    public int?     Rating               { get; set; }
    public decimal? OfferedSalary        { get; set; }
    public DateOnly? OfferDate           { get; set; }
    public DateOnly? ExpectedJoiningDate { get; set; }
    public string?  Notes                { get; set; }
}

public class CandidateDetailDto : CandidateDto
{
    public List<InterviewDto> Interviews { get; set; } = new();
    public List<StageHistoryDto> StageHistory { get; set; } = new();
    public int ProgressPercent { get; set; }
}

public class StageHistoryDto
{
    public string    Stage         { get; set; } = string.Empty;
    public string?   Notes         { get; set; }
    public string    ChangedByName { get; set; } = string.Empty;
    public DateTime  ChangedAt     { get; set; }
}

public class CreateCandidateDto
{
    public string   Name                 { get; set; } = string.Empty;
    public string   Email                { get; set; } = string.Empty;
    public string?  Phone                { get; set; }
    public string?  ResumeUrl            { get; set; }
    public string?  CurrentCompany       { get; set; }
    public decimal? TotalExperienceYears { get; set; }
    public decimal? ExpectedSalary       { get; set; }
    public string?  Source               { get; set; }
    public int?     Rating               { get; set; }
    public string?  Notes                { get; set; }
}

public class UpdateCandidateDto
{
    public string   Name                 { get; set; } = string.Empty;
    public string   Email                { get; set; } = string.Empty;
    public string?  Phone                { get; set; }
    public string?  ResumeUrl            { get; set; }
    public string?  CurrentCompany       { get; set; }
    public decimal? TotalExperienceYears { get; set; }
    public decimal? ExpectedSalary       { get; set; }
    public string?  Source               { get; set; }
    public int?     Rating               { get; set; }
    public string?  Notes                { get; set; }
}

public class ChangeCandidateStageDto
{
    public string    Stage               { get; set; } = string.Empty; // Applied | Screening | Interview | Offered | Hired | Rejected
    public string?   Notes               { get; set; }
    public decimal?  OfferedSalary       { get; set; }
    public DateOnly? OfferDate           { get; set; }
    public DateOnly? ExpectedJoiningDate { get; set; }
}

public class InterviewDto
{
    public int      Id                { get; set; }
    public int      CandidateId       { get; set; }
    public string   CandidateName     { get; set; } = string.Empty;
    public string   JobOpeningTitle   { get; set; } = string.Empty;
    public int      Round             { get; set; }
    public string   Title             { get; set; } = string.Empty;
    public DateTime ScheduledAt       { get; set; }
    public string   Mode              { get; set; } = string.Empty;
    public string?  InterviewerName   { get; set; }
    public int?     InterviewerUserId { get; set; }
    public string   Status            { get; set; } = string.Empty;
    public string?  Feedback          { get; set; }
    public int?     Score             { get; set; }
}

public class CreateInterviewDto
{
    public int      Round             { get; set; } = 1;
    public string   Title             { get; set; } = string.Empty;
    public DateTime ScheduledAt       { get; set; }
    public string   Mode              { get; set; } = "Video";
    public string?  InterviewerName   { get; set; }
    public int?     InterviewerUserId { get; set; }
}

public class UpdateInterviewDto
{
    public int      Round             { get; set; } = 1;
    public string   Title             { get; set; } = string.Empty;
    public DateTime ScheduledAt       { get; set; }
    public string   Mode              { get; set; } = "Video";
    public string?  InterviewerName   { get; set; }
    public int?     InterviewerUserId { get; set; }
}

public class InterviewFeedbackDto
{
    public string  Status   { get; set; } = string.Empty; // Completed | Cancelled | NoShow
    public string? Feedback { get; set; }
    public int?    Score    { get; set; }
}

public class RecruitmentPipelineDto
{
    public int Applied   { get; set; }
    public int Screening { get; set; }
    public int Interview { get; set; }
    public int Offered   { get; set; }
    public int Hired     { get; set; }
    public int Rejected  { get; set; }
}
