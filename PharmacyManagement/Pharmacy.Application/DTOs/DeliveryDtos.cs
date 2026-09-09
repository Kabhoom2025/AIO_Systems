namespace Pharmacy.Application.DTOs;

public class DeliveryDto
{
    public int       Id                 { get; set; }
    public int       SaleId             { get; set; }
    public string    SaleInvoiceNumber  { get; set; } = string.Empty;
    public string    BranchName         { get; set; } = string.Empty;
    public int?      CustomerId         { get; set; }
    public string?   CustomerName       { get; set; }
    public int?      DeliveryStaffId    { get; set; }
    public string?   DeliveryStaffName  { get; set; }
    public string    Address            { get; set; } = string.Empty;
    public DateTime  ScheduledDate      { get; set; }
    public decimal   DeliveryCharge     { get; set; }
    public string    Status             { get; set; } = string.Empty;
    public string    OtpCode            { get; set; } = string.Empty;
    public DateTime? OtpVerifiedDate    { get; set; }
    public string?   Notes              { get; set; }
}

public class CreateDeliveryDto
{
    public int      SaleId         { get; set; }
    public int      BranchId       { get; set; }
    public int?     CustomerId     { get; set; }
    public string   Address        { get; set; } = string.Empty;
    public DateTime ScheduledDate  { get; set; }
    public decimal  DeliveryCharge { get; set; }
    public string?  Notes          { get; set; }
}

public class AssignDeliveryStaffDto
{
    public int DeliveryStaffId { get; set; }
}

public class UpdateDeliveryStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class VerifyDeliveryOtpDto
{
    public string OtpCode { get; set; } = string.Empty;
}
