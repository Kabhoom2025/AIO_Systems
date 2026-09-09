using FoodOrder.Application.DTOs.User;

namespace FoodOrder.Application.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(int? organizationId);
    Task<UserDto> CreateUserAsync(CreateUserDto dto, int? organizationId);
    Task<UserDto> UpdateUserAsync(int userId, UpdateStaffUserDto dto);
    Task ToggleActiveAsync(int userId, int requestingUserId);
    Task DeleteUserAsync(int userId, int requestingUserId);
}
