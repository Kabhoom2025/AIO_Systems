using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IUserService
{
    Task<List<UserSummaryDto>> GetAllAsync(int orgId);
}
