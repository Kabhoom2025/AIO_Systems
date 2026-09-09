namespace FoodOrder.Domain.Entities;

public class SalaryPayment : BaseEntity
{
    public int      UserId     { get; set; }
    public int      Month      { get; set; }
    public int      Year       { get; set; }
    public decimal  GrossPay   { get; set; }
    public decimal  Deductions { get; set; } = 0;
    public decimal  NetPay     { get; set; }
    // Pending | Paid
    public string   Status     { get; set; } = "Pending";
    public DateTime? PaidDate  { get; set; }
    public string?  Notes      { get; set; }

    public User User { get; set; } = null!;
}
