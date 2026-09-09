namespace FoodOrder.Domain.Entities;

public class ShiftDefinition : BaseEntity
{
    public string   Name      { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime   { get; set; }
    public string?  ColorCode { get; set; } = "#1976d2";

    public ICollection<ShiftAssignment> Assignments { get; set; } = new List<ShiftAssignment>();
}
