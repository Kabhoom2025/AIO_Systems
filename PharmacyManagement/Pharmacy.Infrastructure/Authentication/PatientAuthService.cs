using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Authentication;

public class PatientAuthService : IPatientAuthService
{
    private const int OtpValidityMinutes = 5;
    private const int MaxAttempts = 5;

    private readonly IPatientRepository _patientRepo;
    private readonly IOtpChallengeRepository _otpRepo;
    private readonly IOrganizationRepository _orgRepo;
    private readonly IConfiguration _config;

    public PatientAuthService(
        IPatientRepository patientRepo,
        IOtpChallengeRepository otpRepo,
        IOrganizationRepository orgRepo,
        IConfiguration config)
    {
        _patientRepo = patientRepo;
        _otpRepo = otpRepo;
        _orgRepo = orgRepo;
        _config = config;
    }

    public async Task<RequestOtpResponseDto> RequestOtpAsync(string phone)
    {
        phone = phone.Trim();

        var orgIds = await _orgRepo.GetAllActiveIdsAsync();
        var orgId = orgIds.FirstOrDefault();
        if (orgId == 0)
            throw new InvalidOperationException("No active hospital organization is configured.");

        var patient = await _patientRepo.GetByPhoneAsync(orgId, phone);
        if (patient == null)
        {
            patient = new Patient
            {
                OrganizationId = orgId,
                Name = string.Empty,
                Phone = phone,
                IsActive = true,
                IsVerified = false
            };
            _patientRepo.Add(patient);
            await _patientRepo.SaveChangesAsync();
        }

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        _otpRepo.Add(new OtpChallenge
        {
            Phone = phone,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpValidityMinutes)
        });
        await _otpRepo.SaveChangesAsync();

        return new RequestOtpResponseDto
        {
            Message = $"OTP sent to {phone}. Valid for {OtpValidityMinutes} minutes.",
            DevOtp = code
        };
    }

    public async Task<PatientLoginResponseDto?> VerifyOtpAsync(string phone, string code)
    {
        phone = phone.Trim();

        var challenge = await _otpRepo.GetActiveByPhoneAsync(phone);
        if (challenge == null) return null;

        if (challenge.Code != code)
        {
            challenge.Attempts++;
            if (challenge.Attempts >= MaxAttempts) challenge.Consumed = true;
            await _otpRepo.SaveChangesAsync();
            return null;
        }

        challenge.Consumed = true;
        await _otpRepo.SaveChangesAsync();

        var orgIds = await _orgRepo.GetAllActiveIdsAsync();
        var orgId = orgIds.FirstOrDefault();
        var patient = await _patientRepo.GetByPhoneAsync(orgId, phone)
            ?? throw new InvalidOperationException("Patient record not found for verified phone.");

        patient.IsVerified = true;
        patient.UpdatedDate = DateTime.UtcNow;
        await _patientRepo.SaveChangesAsync();

        var org = await _orgRepo.GetByIdAsync(patient.OrganizationId);

        var jwt = _config.GetSection("JwtSettings");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, patient.Id.ToString()),
            new Claim("phone", patient.Phone ?? string.Empty),
            new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(patient.Name) ? patient.Phone ?? string.Empty : patient.Name),
            new Claim("organizationId", patient.OrganizationId.ToString()),
            new Claim("tokenType", "patient")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new PatientLoginResponseDto
        {
            Token            = new JwtSecurityTokenHandler().WriteToken(token),
            PatientId        = patient.Id,
            Name             = patient.Name,
            Phone            = patient.Phone ?? string.Empty,
            OrganizationId   = patient.OrganizationId,
            OrganizationName = org?.Name ?? string.Empty
        };
    }
}
