using FoodOrder.Domain.Enums;

namespace FoodOrder.Domain.Entities;

public class Promotion : BaseEntity
{
    public string  Name          { get; set; } = string.Empty;
    public string? Description   { get; set; }
    public PromotionType PromotionType { get; set; } = PromotionType.Coupon;
    public DiscountType  DiscountType  { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }

    // Code-based (Coupon / PromoCode / GiftCard)
    public string? Code { get; set; }

    // Order constraints
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscount   { get; set; }

    // Usage limits
    public int? UsageLimit { get; set; }
    public int  UsedCount  { get; set; } = 0;
    public int? UsageLimitPerCustomer { get; set; }

    // Date range (FestivalOffer, PromoCode, etc.)
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate   { get; set; }

    // Happy Hour time window  (stored as "HH:mm" strings)
    public string? HappyHourStart { get; set; }
    public string? HappyHourEnd   { get; set; }
    public string? HappyHourDays  { get; set; }  // "Mon,Tue,Wed,Thu,Fri"

    // BOGO / Combo
    public string? ApplicableItemIds { get; set; }  // JSON array of food item IDs
    public int?    BuyQty            { get; set; }
    public int?    GetQty            { get; set; }

    // Gift Card
    public decimal? GiftCardBalance { get; set; }
    public decimal? GiftCardUsed    { get; set; } = 0;

    // Loyalty Reward
    public decimal? SpendThreshold     { get; set; }
    public decimal? PointsMultiplierVal { get; set; }

    public bool    IsActive  { get; set; } = true;
    public bool    IsPublic  { get; set; } = true;   // show on customer menu page
    public string? BannerColor { get; set; }          // hex color for slide card
    public string? BadgeIcon   { get; set; }          // emoji icon

    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
