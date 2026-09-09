namespace FoodOrder.Application.DTOs.Table;

public class UpdateTableDto
{
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public string Hall { get; set; } = "AC";
    public bool IsActive { get; set; }
}
