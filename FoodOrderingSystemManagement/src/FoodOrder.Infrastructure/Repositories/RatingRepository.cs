using FoodOrder.Application.DTOs.Rating;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context) => _context = context;

    public async Task<bool> HasRatedAsync(string sessionId, int foodItemId) =>
        await _context.FoodItemRatings.AnyAsync(r =>
            r.SessionId == sessionId && r.FoodItemId == foodItemId);

    public async Task AddAsync(FoodItemRating rating) =>
        await _context.FoodItemRatings.AddAsync(rating);

    public Task<int> SaveChangesAsync() =>
        _context.SaveChangesAsync();

    public async Task<List<RatingStatsDto>> GetAllStatsAsync() =>
        await _context.FoodItemRatings
            .GroupBy(r => r.FoodItemId)
            .Select(g => new RatingStatsDto
            {
                FoodItemId    = g.Key,
                AverageRating = g.Average(r => (double)r.Rating),
                RatingCount   = g.Count(),
            })
            .ToListAsync();

    public async Task<RatingStatsDto?> GetStatsAsync(int foodItemId)
    {
        var ratings = await _context.FoodItemRatings
            .Where(r => r.FoodItemId == foodItemId)
            .ToListAsync();

        if (ratings.Count == 0) return null;

        return new RatingStatsDto
        {
            FoodItemId    = foodItemId,
            AverageRating = ratings.Average(r => (double)r.Rating),
            RatingCount   = ratings.Count,
        };
    }

    public async Task<List<RecommendedItemDto>> GetRecommendedAsync(int topN = 8)
    {
        var raw = await _context.FoodItems
            .Include(f => f.Category)
            .Include(f => f.Ratings)
            .Include(f => f.OrderItems)
            .Where(f => f.IsAvailable)
            .ToListAsync();

        if (raw.Count == 0)
            return [];

        int maxOrders = raw.Max(f => f.OrderItems.Count);

        return raw
            .Select(f =>
            {
                double avgRating  = f.Ratings.Count > 0 ? f.Ratings.Average(r => (double)r.Rating) : 0;
                int    orders     = f.OrderItems.Count;
                double rScore     = avgRating / 5.0;
                double oScore     = maxOrders > 0
                                        ? Math.Log(orders + 1) / Math.Log(maxOrders + 1)
                                        : 0;
                double score      = rScore * 0.5 + oScore * 0.5;

                string tag = avgRating >= 4.5 ? "Top Rated"
                           : orders    >= 20  ? "Most Ordered"
                           : score     >= 0.4 ? "Popular"
                                              : "Trending";

                return new RecommendedItemDto
                {
                    FoodItemId        = f.Id,
                    ItemName          = f.ItemName,
                    CategoryName      = f.Category?.CategoryName ?? string.Empty,
                    Price             = f.Price,
                    Image             = f.Image,
                    AverageRating     = avgRating,
                    RatingCount       = f.Ratings.Count,
                    OrderCount        = orders,
                    Score             = score,
                    RecommendationTag = tag,
                };
            })
            .Where(f => f.RatingCount > 0 || f.OrderCount >= 3)
            .OrderByDescending(f => f.Score)
            .Take(topN)
            .ToList();
    }
}
