namespace Pharmacy.Application.DTOs;

public class RequestOtpDto
{
    public string Phone { get; set; } = string.Empty;
}

public class RequestOtpResponseDto
{
    public string  Message { get; set; } = string.Empty;
    // Dev-only: there is no SMS gateway configured in this system, so the OTP is
    // returned directly here for testing. Remove once real SMS delivery exists.
    public string? DevOtp  { get; set; }
}

public class VerifyOtpDto
{
    public string Phone { get; set; } = string.Empty;
    public string Code  { get; set; } = string.Empty;
}

public class PatientLoginResponseDto
{
    public string Token            { get; set; } = string.Empty;
    public int    PatientId        { get; set; }
    public string Name             { get; set; } = string.Empty;
    public string Phone            { get; set; } = string.Empty;
    public int    OrganizationId   { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
}
