using FoodOrder.Application.DTOs.Rating;

namespace FoodOrder.Application.Interfaces.Services;

public interface IRatingService
{
    Task<RatingStatsDto>           SubmitRatingAsync(SubmitRatingRequest request);
    Task<List<RatingStatsDto>>     GetAllStatsAsync();
    Task<RatingStatsDto?>          GetStatsAsync(int foodItemId);
    Task<List<RecommendedItemDto>> GetRecommendedAsync();
}
