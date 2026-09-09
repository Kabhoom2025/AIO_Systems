using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.Promotion;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _repo;
    private readonly ICurrentUserContext _currentUser;

    public PromotionService(IPromotionRepository repo, ICurrentUserContext currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PromotionDto>> GetAllAsync()
    {
        var promos = await _repo.GetAllAsync();
        return promos.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<PromotionDto> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Promotion", id);
        return MapToDto(p);
    }

    public async Task<IReadOnlyList<PromotionDto>> GetActiveAsync()
    {
        var promos = await _repo.GetActiveAsync();
        return promos.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<PromotionDto>> GetPublicActiveAsync(int organizationId)
    {
        var promos = await _repo.GetPublicActiveAsync(organizationId);
        return promos.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var existing = await _repo.GetByCodeAsync(dto.Code.Trim().ToUpperInvariant());
            if (existing != null)
                throw new AppException($"A promotion with code '{dto.Code}' already exists.");
        }

        var promotionType = Enum.TryParse<PromotionType>(dto.PromotionType, out var pt) ? pt : PromotionType.Coupon;
        var discountType  = Enum.TryParse<DiscountType>(dto.DiscountType, out var dt) ? dt : DiscountType.Percentage;

        var promotion = new Promotion
        {
            OrganizationId = TenantResolution.ResolveWriteOrganizationId(_currentUser),
            Name          = dto.Name.Trim(),
            Description   = dto.Description?.Trim(),
            PromotionType = promotionType,
            DiscountType  = discountType,
            DiscountValue = dto.DiscountValue,
            Code          = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpperInvariant(),
            MinOrderValue = dto.MinOrderValue,
            MaxDiscount   = dto.MaxDiscount,
            UsageLimit    = dto.UsageLimit,
            UsageLimitPerCustomer = dto.UsageLimitPerCustomer,
            StartDate     = ToUtc(dto.StartDate),
            EndDate       = ToUtc(dto.EndDate),
            HappyHourStart = dto.HappyHourStart,
            HappyHourEnd   = dto.HappyHourEnd,
            HappyHourDays  = dto.HappyHourDays,
            ApplicableItemIds = dto.ApplicableItemIds,
            BuyQty        = dto.BuyQty,
            GetQty        = dto.GetQty,
            GiftCardBalance    = dto.GiftCardBalance,
            GiftCardUsed       = promotionType == PromotionType.GiftCard ? 0 : null,
            SpendThreshold     = dto.SpendThreshold,
            PointsMultiplierVal = dto.PointsMultiplierVal,
            IsActive   = dto.IsActive,
            IsPublic   = dto.IsPublic,
            BannerColor = dto.BannerColor,
            BadgeIcon   = dto.BadgeIcon,
            CreatedDate = DateTime.UtcNow,
        };

        await _repo.AddAsync(promotion);
        await _repo.SaveChangesAsync();
        return MapToDto(promotion);
    }

    public async Task<PromotionDto> UpdateAsync(int id, CreatePromotionDto dto)
    {
        var promotion = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Promotion", id);

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var normalised = dto.Code.Trim().ToUpperInvariant();
            var existing = await _repo.GetByCodeAsync(normalised);
            if (existing != null && existing.Id != id)
                throw new AppException($"A promotion with code '{dto.Code}' already exists.");
        }

        promotion.Name          = dto.Name.Trim();
        promotion.Description   = dto.Description?.Trim();
        promotion.PromotionType = Enum.TryParse<PromotionType>(dto.PromotionType, out var pt) ? pt : promotion.PromotionType;
        promotion.DiscountType  = Enum.TryParse<DiscountType>(dto.DiscountType,  out var dt) ? dt : promotion.DiscountType;
        promotion.DiscountValue = dto.DiscountValue;
        promotion.Code          = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpperInvariant();
        promotion.MinOrderValue = dto.MinOrderValue;
        promotion.MaxDiscount   = dto.MaxDiscount;
        promotion.UsageLimit    = dto.UsageLimit;
        promotion.UsageLimitPerCustomer = dto.UsageLimitPerCustomer;
        promotion.StartDate     = ToUtc(dto.StartDate);
        promotion.EndDate       = ToUtc(dto.EndDate);
        promotion.HappyHourStart = dto.HappyHourStart;
        promotion.HappyHourEnd   = dto.HappyHourEnd;
        promotion.HappyHourDays  = dto.HappyHourDays;
        promotion.ApplicableItemIds = dto.ApplicableItemIds;
        promotion.BuyQty        = dto.BuyQty;
        promotion.GetQty        = dto.GetQty;
        promotion.GiftCardBalance    = dto.GiftCardBalance;
        promotion.SpendThreshold     = dto.SpendThreshold;
        promotion.PointsMultiplierVal = dto.PointsMultiplierVal;
        promotion.IsActive   = dto.IsActive;
        promotion.IsPublic   = dto.IsPublic;
        promotion.BannerColor = dto.BannerColor;
        promotion.BadgeIcon   = dto.BadgeIcon;

        _repo.Update(promotion);
        await _repo.SaveChangesAsync();
        return MapToDto(promotion);
    }

    public async Task DeleteAsync(int id)
    {
        var promotion = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Promotion", id);
        _repo.Delete(promotion);
        await _repo.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var promotion = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Promotion", id);
        promotion.IsActive = !promotion.IsActive;
        _repo.Update(promotion);
        await _repo.SaveChangesAsync();
        return promotion.IsActive;
    }

    public async Task<ApplyPromotionResultDto> ApplyCodeAsync(ApplyPromoCodeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return Invalid("Please enter a promo code.");

        var promo = await _repo.GetByCodeAsync(dto.Code.Trim().ToUpperInvariant());
        if (promo == null)
            return Invalid("Invalid promo code.");

        if (!promo.IsActive)
            return Invalid("This promotion is no longer active.");

        var now = DateTime.UtcNow;

        if (promo.StartDate.HasValue && now < promo.StartDate.Value)
            return Invalid($"This offer starts on {promo.StartDate.Value:dd MMM yyyy}.");

        if (promo.EndDate.HasValue && now > promo.EndDate.Value)
            return Invalid("This promotion has expired.");

        if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
            return Invalid("This promotion has reached its usage limit.");

        if (promo.MinOrderValue.HasValue && dto.OrderTotal < promo.MinOrderValue.Value)
            return Invalid($"Minimum order value of ₹{promo.MinOrderValue.Value:0} required.");

        // Gift card — check remaining balance
        if (promo.PromotionType == PromotionType.GiftCard)
        {
            var remaining = (promo.GiftCardBalance ?? 0) - (promo.GiftCardUsed ?? 0);
            if (remaining <= 0) return Invalid("This gift card has no remaining balance.");
            var discount = Math.Min(remaining, dto.OrderTotal);
            return Valid(promo, discount);
        }

        var discountAmount = promo.DiscountType switch
        {
            DiscountType.Percentage => Math.Round(dto.OrderTotal * promo.DiscountValue / 100, 2),
            DiscountType.FlatAmount => promo.DiscountValue,
            _ => 0m,
        };

        if (promo.MaxDiscount.HasValue)
            discountAmount = Math.Min(discountAmount, promo.MaxDiscount.Value);

        discountAmount = Math.Min(discountAmount, dto.OrderTotal);

        return Valid(promo, discountAmount);
    }

    private static ApplyPromotionResultDto Invalid(string message) =>
        new() { IsValid = false, Message = message, DiscountAmount = 0 };

    private static ApplyPromotionResultDto Valid(Promotion p, decimal amount) =>
        new()
        {
            IsValid       = true,
            Message       = $"'{p.Name}' applied! You save ₹{amount:0.00}.",
            DiscountAmount = amount,
            DiscountType  = p.DiscountType.ToString(),
            DiscountValue = p.DiscountValue,
            PromotionId   = p.Id,
            PromotionName = p.Name,
        };

    private static string ResolveStatus(Promotion p)
    {
        if (!p.IsActive) return "Inactive";
        var now = DateTime.UtcNow;
        if (p.StartDate.HasValue && now < p.StartDate.Value) return "Upcoming";
        if (p.EndDate.HasValue   && now > p.EndDate.Value)   return "Expired";
        if (p.UsageLimit.HasValue && p.UsedCount >= p.UsageLimit.Value) return "Exhausted";
        return "Active";
    }

    private static PromotionDto MapToDto(Promotion p) => new()
    {
        Id            = p.Id,
        Name          = p.Name,
        Description   = p.Description,
        PromotionType = p.PromotionType.ToString(),
        DiscountType  = p.DiscountType.ToString(),
        DiscountValue = p.DiscountValue,
        Code          = p.Code,
        MinOrderValue = p.MinOrderValue,
        MaxDiscount   = p.MaxDiscount,
        UsageLimit    = p.UsageLimit,
        UsedCount     = p.UsedCount,
        UsageLimitPerCustomer = p.UsageLimitPerCustomer,
        StartDate     = p.StartDate,
        EndDate       = p.EndDate,
        HappyHourStart = p.HappyHourStart,
        HappyHourEnd   = p.HappyHourEnd,
        HappyHourDays  = p.HappyHourDays,
        ApplicableItemIds = p.ApplicableItemIds,
        BuyQty        = p.BuyQty,
        GetQty        = p.GetQty,
        GiftCardBalance     = p.GiftCardBalance,
        GiftCardUsed        = p.GiftCardUsed,
        SpendThreshold      = p.SpendThreshold,
        PointsMultiplierVal = p.PointsMultiplierVal,
        IsActive   = p.IsActive,
        IsPublic   = p.IsPublic,
        BannerColor = p.BannerColor,
        BadgeIcon   = p.BadgeIcon,
        Status      = ResolveStatus(p),
        CreatedDate = p.CreatedDate,
    };

    // Npgsql 6+ requires DateTime.Kind = Utc for 'timestamp with time zone' columns.
    // Dates from the frontend arrive as Kind=Unspecified; this normalises them.
    private static DateTime? ToUtc(DateTime? dt) =>
        dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;
}
