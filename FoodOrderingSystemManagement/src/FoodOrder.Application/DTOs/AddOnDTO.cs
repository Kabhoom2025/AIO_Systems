namespace FoodOrder.Application.DTOs;

public class AddOnDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateAddOnRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; }
}

public class UpdateAddOnRequest : CreateAddOnRequest
{
    public bool IsAvailable { get; set; } = true;
}
