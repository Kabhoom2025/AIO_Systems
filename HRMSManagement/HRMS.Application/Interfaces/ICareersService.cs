using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface ICareersService
{
    Task<PublicOrgDto> GetOrgAsync(string orgCode);
    Task<List<PublicJobOpeningDto>> GetOpeningsAsync(string orgCode);
    Task<PublicJobOpeningDetailDto> GetOpeningAsync(string orgCode, int id);
    Task<ApplyResultDto> ApplyAsync(string orgCode, int openingId, ApplyToJobDto dto);
    Task<TrackingStatusDto> TrackAsync(string token);
}
