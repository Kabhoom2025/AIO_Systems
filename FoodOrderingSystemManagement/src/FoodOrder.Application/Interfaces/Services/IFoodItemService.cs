using FoodOrder.Application.DTOs.FoodItem;

namespace FoodOrder.Application.Interfaces.Services;

public interface IFoodItemService
{
    Task<IReadOnlyList<FoodItemDto>> GetAllAsync();
    Task<IReadOnlyList<FoodItemDto>> GetAllByBranchAsync(int branchId);
    Task<IReadOnlyList<FoodItemDto>> GetByCategoryAsync(int categoryId);
    Task<FoodItemDto> GetByIdAsync(int id);
    Task<FoodItemDto> CreateAsync(CreateFoodItemDto dto);
    Task<FoodItemDto> UpdateAsync(int id, UpdateFoodItemDto dto);
    Task DeleteAsync(int id);
    Task<FoodItemDto?> GetByBarcodeAsync(string barcode);
}
