namespace FoodOrder.Application.DTOs.Table;

public class CreateTableDto
{
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public string Hall { get; set; } = "AC";
}
