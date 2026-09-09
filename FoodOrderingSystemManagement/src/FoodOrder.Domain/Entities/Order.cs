using FoodOrder.Domain.Enums;

namespace FoodOrder.Domain.Entities;

public class Order : BaseEntity
{
    public int BranchId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? CashierId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal GrandTotal { get; set; }
    public OrderStatus Status    { get; set; } = OrderStatus.Pending;
    public OrderType   OrderType { get; set; } = OrderType.DineIn;
    public int?    TableId         { get; set; }
    public string? CustomerPhone   { get; set; }
    public string? DeliveryAddress { get; set; }
    public decimal DeliveryCharge  { get; set; } = 0;
    public int? CustomerId      { get; set; }
    public int  PointsEarned    { get; set; } = 0;
    public int  PointsRedeemed  { get; set; } = 0;

    public Branch   Branch     { get; set; } = null!;
    public User?    Cashier    { get; set; }
    public Table?   Table      { get; set; }
    public Customer? Customer  { get; set; }
    public ICollection<OrderItem>          OrderItems          { get; set; } = new List<OrderItem>();
    public ICollection<PointsTransaction>  PointsTransactions  { get; set; } = new List<PointsTransaction>();
}
