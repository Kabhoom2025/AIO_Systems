using FoodOrder.Application.DTOs.Rating;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IRatingRepository
{
    Task<bool>   HasRatedAsync(string sessionId, int foodItemId);
    Task         AddAsync(FoodItemRating rating);
    Task<int>    SaveChangesAsync();
    Task<List<RatingStatsDto>>    GetAllStatsAsync();
    Task<RatingStatsDto?>         GetStatsAsync(int foodItemId);
    Task<List<RecommendedItemDto>> GetRecommendedAsync(int topN = 8);
}
