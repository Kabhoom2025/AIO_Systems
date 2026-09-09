using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Services;

public class AiInsightsService : IAiInsightsService
{
    private const int VelocityWindowDays = 30;
    private const int ReorderTargetDays = 21; // ~2 weeks lead time + 1 week safety buffer
    private const decimal UrgentDaysOfStockThreshold = 14;
    private const int ExpiryLookaheadDays = 90;

    private readonly PharmacyDbContext _ctx;
    private readonly IMedicineRepository _medicineRepo;

    public AiInsightsService(PharmacyDbContext ctx, IMedicineRepository medicineRepo)
    {
        _ctx = ctx;
        _medicineRepo = medicineRepo;
    }

    private async Task<Dictionary<int, decimal>> GetVelocityMapAsync(int orgId)
    {
        var since = DateTime.UtcNow.AddDays(-VelocityWindowDays);
        var sold = await _ctx.SaleItems
            .Where(si => si.Sale.OrganizationId == orgId && si.Sale.SaleDate >= since)
            .GroupBy(si => si.MedicineId)
            .Select(g => new { MedicineId = g.Key, TotalQty = g.Sum(si => si.Quantity) })
            .ToListAsync();

        return sold.ToDictionary(s => s.MedicineId, s => s.TotalQty / (decimal)VelocityWindowDays);
    }

    public async Task<List<ReorderSuggestionDto>> GetReorderSuggestionsAsync(int orgId)
    {
        var velocity = await GetVelocityMapAsync(orgId);
        var medicines = await _ctx.Medicines
            .Include(m => m.Batches)
            .Where(m => m.OrganizationId == orgId && m.IsActive)
            .ToListAsync();

        var suggestions = new List<ReorderSuggestionDto>();
        foreach (var m in medicines)
        {
            var currentStock = m.Batches.Sum(b => b.CurrentQuantity);
            var avgDaily = velocity.GetValueOrDefault(m.Id, 0);
            decimal? daysOfStockLeft = avgDaily > 0 ? currentStock / avgDaily : null;

            var isUrgent = currentStock <= m.ReorderLevel || (daysOfStockLeft.HasValue && daysOfStockLeft <= UrgentDaysOfStockThreshold);
            if (!isUrgent) continue;

            int suggestedQty = avgDaily > 0
                ? Math.Max(0, (int)Math.Ceiling(avgDaily * ReorderTargetDays) - currentStock)
                : Math.Max(0, m.ReorderLevel * 2 - currentStock);

            suggestions.Add(new ReorderSuggestionDto
            {
                MedicineId          = m.Id,
                MedicineName        = m.Name,
                CurrentStock        = currentStock,
                AvgDailySales       = Math.Round(avgDaily, 2),
                DaysOfStockLeft     = daysOfStockLeft.HasValue ? Math.Round(daysOfStockLeft.Value, 1) : null,
                SuggestedReorderQty = suggestedQty
            });
        }

        return suggestions
            .OrderBy(s => s.DaysOfStockLeft ?? decimal.MaxValue)
            .ToList();
    }

    public async Task<List<ExpiryRiskDto>> GetExpiryRiskAsync(int orgId)
    {
        var velocity = await GetVelocityMapAsync(orgId);
        var batches = await _medicineRepo.GetExpiryAlertsAsync(orgId, ExpiryLookaheadDays);

        var results = new List<ExpiryRiskDto>();
        foreach (var b in batches)
        {
            var daysToExpiry = (b.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
            var avgDaily = velocity.GetValueOrDefault(b.MedicineId, 0);
            decimal? projectedDaysToSellOut = avgDaily > 0 ? b.CurrentQuantity / avgDaily : null;

            string riskLevel;
            if (daysToExpiry < 0) riskLevel = "Expired";
            else if (projectedDaysToSellOut.HasValue && projectedDaysToSellOut > daysToExpiry) riskLevel = "High";
            else if (daysToExpiry <= 30) riskLevel = "Medium";
            else riskLevel = "Low";

            results.Add(new ExpiryRiskDto
            {
                MedicineId             = b.MedicineId,
                MedicineName           = b.Medicine.Name,
                BatchId                = b.Id,
                BatchNumber            = b.BatchNumber,
                Quantity               = b.CurrentQuantity,
                DaysToExpiry           = daysToExpiry,
                AvgDailySales          = Math.Round(avgDaily, 2),
                ProjectedDaysToSellOut = projectedDaysToSellOut.HasValue ? Math.Round(projectedDaysToSellOut.Value, 1) : null,
                RiskLevel              = riskLevel
            });
        }

        return results.OrderBy(r => r.DaysToExpiry).ToList();
    }
}
