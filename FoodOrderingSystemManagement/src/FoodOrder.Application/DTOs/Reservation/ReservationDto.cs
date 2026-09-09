namespace FoodOrder.Application.DTOs.Reservation;

public class ReservationDto
{
    public int      Id                  { get; set; }
    public int      TableId             { get; set; }
    public int      TableNumber         { get; set; }
    public string   Hall                { get; set; } = string.Empty;
    public string   GuestName           { get; set; } = string.Empty;
    public string   GuestPhone          { get; set; } = string.Empty;
    public int      PartySize           { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public string?  Notes               { get; set; }
    public string   Status              { get; set; } = "Pending";
    public DateTime CreatedDate         { get; set; }
}

public class CreateReservationRequest
{
    public int      TableId             { get; set; }
    public string   GuestName           { get; set; } = string.Empty;
    public string   GuestPhone          { get; set; } = string.Empty;
    public int      PartySize           { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public string?  Notes               { get; set; }
}

public class UpdateReservationRequest
{
    public string   GuestName           { get; set; } = string.Empty;
    public string   GuestPhone          { get; set; } = string.Empty;
    public int      PartySize           { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public string?  Notes               { get; set; }
}

public class UpdateReservationStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
