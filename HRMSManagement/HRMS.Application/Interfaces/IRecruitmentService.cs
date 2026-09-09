using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IRecruitmentService
{
    // Job openings
    Task<List<JobOpeningDto>> GetOpeningsAsync(int orgId, string? status);
    Task<JobOpeningDto> GetOpeningAsync(int orgId, int id);
    Task<JobOpeningDto> CreateOpeningAsync(int orgId, CreateJobOpeningDto dto);
    Task<JobOpeningDto> UpdateOpeningAsync(int orgId, int id, UpdateJobOpeningDto dto);
    Task DeleteOpeningAsync(int orgId, int id);

    // Candidates
    Task<List<CandidateDto>> GetCandidatesAsync(int orgId, int openingId);
    Task<CandidateDetailDto> GetCandidateAsync(int orgId, int id);
    Task<CandidateDto> CreateCandidateAsync(int orgId, int openingId, CreateCandidateDto dto);
    Task<CandidateDto> UpdateCandidateAsync(int orgId, int id, UpdateCandidateDto dto);
    Task<CandidateDto> ChangeStageAsync(int orgId, int id, ChangeCandidateStageDto dto, string changedByName);
    Task DeleteCandidateAsync(int orgId, int id);

    // Interviews
    Task<List<InterviewDto>> GetUpcomingInterviewsAsync(int orgId);
    Task<InterviewDto> ScheduleInterviewAsync(int orgId, int candidateId, CreateInterviewDto dto);
    Task<InterviewDto> UpdateInterviewAsync(int orgId, int id, UpdateInterviewDto dto);
    Task<InterviewDto> SubmitInterviewFeedbackAsync(int orgId, int id, InterviewFeedbackDto dto);

    // Pipeline
    Task<RecruitmentPipelineDto> GetPipelineAsync(int orgId);
}
