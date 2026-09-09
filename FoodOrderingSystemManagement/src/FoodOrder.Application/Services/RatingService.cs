using FoodOrder.Application.DTOs.Rating;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _repo;

    public RatingService(IRatingRepository repo) => _repo = repo;

    public async Task<RatingStatsDto> SubmitRatingAsync(SubmitRatingRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new AppException("Rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(request.SessionId))
            throw new AppException("Session ID is required.");

        if (await _repo.HasRatedAsync(request.SessionId, request.FoodItemId))
            throw new AppException("You have already rated this item.");

        var rating = new FoodItemRating
        {
            FoodItemId   = request.FoodItemId,
            Rating       = request.Rating,
            Comment      = request.Comment?.Trim(),
            SessionId    = request.SessionId,
            TableNumber  = request.TableNumber,
        };

        await _repo.AddAsync(rating);
        await _repo.SaveChangesAsync();

        return await _repo.GetStatsAsync(request.FoodItemId)
               ?? new RatingStatsDto { FoodItemId = request.FoodItemId, AverageRating = request.Rating, RatingCount = 1 };
    }

    public Task<List<RatingStatsDto>>     GetAllStatsAsync()    => _repo.GetAllStatsAsync();
    public Task<RatingStatsDto?>          GetStatsAsync(int id) => _repo.GetStatsAsync(id);
    public Task<List<RecommendedItemDto>> GetRecommendedAsync() => _repo.GetRecommendedAsync();
}
