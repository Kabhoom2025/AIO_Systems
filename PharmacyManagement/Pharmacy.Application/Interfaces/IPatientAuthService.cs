using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IPatientAuthService
{
    Task<RequestOtpResponseDto> RequestOtpAsync(string phone);
    Task<PatientLoginResponseDto?> VerifyOtpAsync(string phone, string code);
}
