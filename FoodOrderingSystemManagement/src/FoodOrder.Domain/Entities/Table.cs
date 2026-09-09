namespace FoodOrder.Domain.Entities;

public class Table : BaseEntity
{
    public int BranchId { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public string Hall { get; set; } = "AC";
    public bool IsActive { get; set; } = true;

    public Branch Branch { get; set; } = null!;
    public ICollection<Order>            Orders       { get; set; } = new List<Order>();
    public ICollection<TableReservation> Reservations { get; set; } = new List<TableReservation>();
}
