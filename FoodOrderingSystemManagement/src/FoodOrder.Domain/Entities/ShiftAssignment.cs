namespace FoodOrder.Domain.Entities;

public class ShiftAssignment : BaseEntity
{
    public int      UserId  { get; set; }
    public int      ShiftId { get; set; }
    public DateTime Date    { get; set; }

    public User            User  { get; set; } = null!;
    public ShiftDefinition Shift { get; set; } = null!;
}
