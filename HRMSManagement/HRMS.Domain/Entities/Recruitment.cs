namespace HRMS.Domain.Entities;

public class JobOpening : BaseEntity
{
    public int      OrganizationId  { get; set; }
    public int      DepartmentId    { get; set; }
    public int      DesignationId   { get; set; }
    public string   Title           { get; set; } = string.Empty;
    public string?  Description     { get; set; }
    public int      Vacancies       { get; set; } = 1;
    public string?  Location        { get; set; }
    public string   EmploymentType  { get; set; } = "FullTime";
    public decimal? MinExperienceYears { get; set; }
    public decimal? MaxExperienceYears { get; set; }
    public decimal? SalaryRangeFrom { get; set; }
    public decimal? SalaryRangeTo   { get; set; }
    public string   Status          { get; set; } = "Open"; // Open | OnHold | Closed
    public DateOnly PostedDate      { get; set; }
    public DateOnly? ClosingDate    { get; set; }

    public Organization Organization { get; set; } = null!;
    public Department   Department   { get; set; } = null!;
    public Designation  Designation  { get; set; } = null!;
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}

public class Candidate : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      JobOpeningId   { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string   Email          { get; set; } = string.Empty;
    public string?  Phone          { get; set; }
    public string?  ResumeUrl      { get; set; }
    public string?  CurrentCompany { get; set; }
    public decimal? TotalExperienceYears { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string?  Source         { get; set; } // Portal | Referral | LinkedIn | Agency …
    public string   Stage          { get; set; } = "Applied"; // Applied | Screening | Interview | Offered | Hired | Rejected
    public int?     Rating         { get; set; } // 1-5
    public decimal? OfferedSalary  { get; set; }
    public DateOnly? OfferDate     { get; set; }
    public DateOnly? ExpectedJoiningDate { get; set; }
    public string?  Notes          { get; set; }

    /// <summary>Opaque token shared with the candidate so they can check their application status without logging in.</summary>
    public string   TrackingToken  { get; set; } = string.Empty;

    public Organization Organization { get; set; } = null!;
    public JobOpening   JobOpening   { get; set; } = null!;
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    public ICollection<CandidateStageHistory> StageHistory { get; set; } = new List<CandidateStageHistory>();
}

/// <summary>One entry per stage transition, powering both the HR pipeline progress view and the public candidate tracking page.</summary>
public class CandidateStageHistory : BaseEntity
{
    public int     CandidateId    { get; set; }
    public string  Stage          { get; set; } = string.Empty;
    public string? Notes          { get; set; }
    /// <summary>Who made this change — an HR user's display name, or "Candidate" for the initial application.</summary>
    public string  ChangedByName  { get; set; } = string.Empty;

    public Candidate Candidate { get; set; } = null!;
}

public class Interview : BaseEntity
{
    public int      CandidateId     { get; set; }
    public int      Round           { get; set; } = 1;
    public string   Title           { get; set; } = string.Empty; // e.g. Technical Round 1
    public DateTime ScheduledAt     { get; set; }
    public string   Mode            { get; set; } = "Video"; // InPerson | Video | Phone
    public string?  InterviewerName { get; set; }
    public int?     InterviewerUserId { get; set; }
    public string   Status          { get; set; } = "Scheduled"; // Scheduled | Completed | Cancelled | NoShow
    public string?  Feedback        { get; set; }
    public int?     Score           { get; set; } // 1-10

    public Candidate Candidate       { get; set; } = null!;
    public User?     InterviewerUser { get; set; }
}
