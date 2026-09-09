using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IAddOnRepository
{
    Task<IReadOnlyList<AddOn>> GetAllAsync();
    Task<AddOn?> GetByIdAsync(int id);
    Task<IReadOnlyList<AddOn>> GetByFoodItemIdAsync(int foodItemId);
    Task AddAsync(AddOn addOn);
    Task UpdateAsync(AddOn addOn);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}
