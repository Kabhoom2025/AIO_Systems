namespace HRMS.Application.DTOs;

public class AssetDto
{
    public int       Id                    { get; set; }
    public string    AssetTag              { get; set; } = string.Empty;
    public string    Name                  { get; set; } = string.Empty;
    public string    Category              { get; set; } = string.Empty;
    public string?   SerialNumber          { get; set; }
    public DateOnly? PurchaseDate          { get; set; }
    public decimal?  PurchaseCost          { get; set; }
    public DateOnly? WarrantyUntil         { get; set; }
    public string    Condition             { get; set; } = string.Empty;
    public string    Status                { get; set; } = string.Empty;
    public string?   Notes                 { get; set; }
    public int?      BranchId              { get; set; }
    public string?   BranchName            { get; set; }
    public int?      CurrentHolderEmployeeId { get; set; }
    public string?   CurrentHolderName     { get; set; }
}

public class AllocationDto
{
    public int       Id              { get; set; }
    public int       EmployeeId      { get; set; }
    public string    EmployeeName    { get; set; } = string.Empty;
    public DateOnly  AllocatedDate   { get; set; }
    public DateOnly? ReturnedDate    { get; set; }
    public string?   ReturnCondition { get; set; }
    public string?   Notes           { get; set; }
    public int?      AllocatedByUserId { get; set; }
    public string?   AllocatedByName { get; set; }
}

public class AssetDetailDto : AssetDto
{
    public List<AllocationDto> Allocations { get; set; } = new();
}

public class CreateAssetDto
{
    public string    Name          { get; set; } = string.Empty;
    public string    Category      { get; set; } = "Laptop";
    public string?   SerialNumber  { get; set; }
    public DateOnly? PurchaseDate  { get; set; }
    public decimal?  PurchaseCost  { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public string    Condition     { get; set; } = "Good";
    public int?      BranchId      { get; set; }
    public string?   Notes         { get; set; }
}

public class UpdateAssetDto
{
    public string    Name          { get; set; } = string.Empty;
    public string    Category      { get; set; } = "Laptop";
    public string?   SerialNumber  { get; set; }
    public DateOnly? PurchaseDate  { get; set; }
    public decimal?  PurchaseCost  { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public string    Condition     { get; set; } = "Good";
    public string    Status        { get; set; } = "Available";
    public int?      BranchId      { get; set; }
    public string?   Notes         { get; set; }
}

public class AllocateAssetDto
{
    public int     EmployeeId { get; set; }
    public string? Notes      { get; set; }
}

public class ReturnAssetDto
{
    public string? ReturnCondition { get; set; }
    public string? Notes           { get; set; }
}
