namespace FoodOrder.Application.DTOs.Promotion;

public class PromotionDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public string  PromotionType { get; set; } = string.Empty;
    public string  DiscountType  { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public string? Code          { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscount   { get; set; }
    public int?    UsageLimit    { get; set; }
    public int     UsedCount     { get; set; }
    public int?    UsageLimitPerCustomer { get; set; }
    public DateTime? StartDate   { get; set; }
    public DateTime? EndDate     { get; set; }
    public string? HappyHourStart { get; set; }
    public string? HappyHourEnd   { get; set; }
    public string? HappyHourDays  { get; set; }
    public string? ApplicableItemIds { get; set; }
    public int?    BuyQty        { get; set; }
    public int?    GetQty        { get; set; }
    public decimal? GiftCardBalance { get; set; }
    public decimal? GiftCardUsed    { get; set; }
    public decimal? SpendThreshold     { get; set; }
    public decimal? PointsMultiplierVal { get; set; }
    public bool    IsActive   { get; set; }
    public bool    IsPublic   { get; set; }
    public string? BannerColor { get; set; }
    public string? BadgeIcon   { get; set; }
    public string  Status      { get; set; } = string.Empty; // Active/Inactive/Expired/Upcoming
    public DateTime CreatedDate { get; set; }
}

public class CreatePromotionDto
{
    public string  Name         { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public string  PromotionType { get; set; } = "Coupon";
    public string  DiscountType  { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; }
    public string? Code          { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscount   { get; set; }
    public int?    UsageLimit    { get; set; }
    public int?    UsageLimitPerCustomer { get; set; }
    public DateTime? StartDate   { get; set; }
    public DateTime? EndDate     { get; set; }
    public string? HappyHourStart { get; set; }
    public string? HappyHourEnd   { get; set; }
    public string? HappyHourDays  { get; set; }
    public string? ApplicableItemIds { get; set; }
    public int?    BuyQty        { get; set; }
    public int?    GetQty        { get; set; }
    public decimal? GiftCardBalance { get; set; }
    public decimal? SpendThreshold     { get; set; }
    public decimal? PointsMultiplierVal { get; set; }
    public bool    IsActive  { get; set; } = true;
    public bool    IsPublic  { get; set; } = true;
    public string? BannerColor { get; set; }
    public string? BadgeIcon   { get; set; }
}

public class ApplyPromotionResultDto
{
    public bool    IsValid       { get; set; }
    public string  Message       { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public string? DiscountType  { get; set; }
    public decimal DiscountValue { get; set; }
    public int?    PromotionId   { get; set; }
    public string? PromotionName { get; set; }
}

public class ApplyPromoCodeDto
{
    public string  Code       { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
}
