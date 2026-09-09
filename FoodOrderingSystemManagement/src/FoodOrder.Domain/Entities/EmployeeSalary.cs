namespace FoodOrder.Domain.Entities;

public class EmployeeSalary : BaseEntity
{
    public int     UserId       { get; set; }
    public decimal BasicSalary  { get; set; } = 0;
    public decimal Allowances   { get; set; } = 0;
    // Monthly | Weekly
    public string  PayPeriod    { get; set; } = "Monthly";

    public User User { get; set; } = null!;
    public ICollection<SalaryPayment> Payments { get; set; } = new List<SalaryPayment>();
}
