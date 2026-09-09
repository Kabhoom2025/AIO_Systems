namespace FoodOrder.Domain.Entities;

public class AttendanceRecord : BaseEntity
{
    public int      UserId   { get; set; }
    public DateTime Date     { get; set; }
    public TimeSpan? CheckIn  { get; set; }
    public TimeSpan? CheckOut { get; set; }
    // Present | Absent | Late | HalfDay | Leave
    public string   Status   { get; set; } = "Present";
    public string?  Notes    { get; set; }

    public User User { get; set; } = null!;
}
