namespace Pharmacy.Domain.Entities;

public class Organization : BaseEntity
{
    public string  Name        { get; set; } = string.Empty;
    public string? Address     { get; set; }
    public string? Phone       { get; set; }
    public string? Email       { get; set; }
    public string? LicenseNo   { get; set; }
    public string  Currency    { get; set; } = "INR";
    public string? GstNumber   { get; set; }
    public string  InvoiceNumberPrefix { get; set; } = "INV";
    public bool    IsActive    { get; set; } = true;

    public ICollection<Branch>         Branches         { get; set; } = new List<Branch>();
    public ICollection<User>           Users            { get; set; } = new List<User>();
    public ICollection<Medicine>       Medicines        { get; set; } = new List<Medicine>();
    public ICollection<Prescription>   Prescriptions    { get; set; } = new List<Prescription>();
    public ICollection<Supplier>       Suppliers        { get; set; } = new List<Supplier>();
    public ICollection<PurchaseOrder>  PurchaseOrders   { get; set; } = new List<PurchaseOrder>();
    public ICollection<GoodsReceipt>   GoodsReceipts    { get; set; } = new List<GoodsReceipt>();
    public ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
    public ICollection<Doctor>         Doctors          { get; set; } = new List<Doctor>();
    public ICollection<Patient>        Patients         { get; set; } = new List<Patient>();
    public ICollection<Customer>       Customers        { get; set; } = new List<Customer>();
    public ICollection<Sale>           Sales            { get; set; } = new List<Sale>();
    public ICollection<Expense>        Expenses         { get; set; } = new List<Expense>();
    public ICollection<Delivery>       Deliveries       { get; set; } = new List<Delivery>();
}
