namespace FoodOrder.Application.DTOs.Table;

public class TableDto
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public string Hall { get; set; } = "AC";
    public bool IsActive { get; set; }
    public bool IsOccupied { get; set; }
    public bool BillPending { get; set; }
}
