using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface IAddOnService
{
    Task<IReadOnlyList<AddOnDTO>> GetAllAsync();
    Task<IReadOnlyList<AddOnDTO>> GetByFoodItemIdAsync(int foodItemId);
    Task<AddOnDTO> GetByIdAsync(int id);
    Task<AddOnDTO> CreateAsync(CreateAddOnRequest dto);
    Task<AddOnDTO> UpdateAsync(int id, UpdateAddOnRequest dto);
    Task DeleteAsync(int id);
}
