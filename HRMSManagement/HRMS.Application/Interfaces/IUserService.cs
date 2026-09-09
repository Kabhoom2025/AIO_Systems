using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(int orgId);
    Task<UserDto?> GetByIdAsync(int orgId, int id);
    Task<UserDto> CreateAsync(int orgId, CreateUserDto dto);
    Task<UserDto> UpdateAsync(int orgId, int id, UpdateUserDto dto);
    Task DeleteAsync(int orgId, int id);
    Task ResetPasswordAsync(int orgId, int id, ResetUserPasswordDto dto);
}
