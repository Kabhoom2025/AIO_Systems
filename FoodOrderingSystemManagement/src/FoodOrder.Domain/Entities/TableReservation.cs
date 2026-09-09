namespace FoodOrder.Domain.Entities;

public enum ReservationStatus
{
    Pending   = 0,
    Confirmed = 1,
    Seated    = 2,
    Cancelled = 3,
    NoShow    = 4
}

public class TableReservation : BaseEntity
{
    public int    TableId             { get; set; }
    public Table  Table               { get; set; } = null!;
    public string GuestName           { get; set; } = string.Empty;
    public string GuestPhone          { get; set; } = string.Empty;
    public int    PartySize           { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public string? Notes              { get; set; }
    public ReservationStatus Status   { get; set; } = ReservationStatus.Pending;
}
