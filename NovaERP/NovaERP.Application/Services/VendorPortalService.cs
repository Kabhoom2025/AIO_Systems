using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

/// <summary>Self-service orchestration for a logged-in User's own Vendor record — no
/// procurement.view/purchase.view permission is checked anywhere here, mirroring
/// CustomerPortalService's reasoning: those permissions are Admin/Manager-facing, and an
/// external vendor has no procurement.*/purchase.* permission at all, so this service scopes
/// every query to "mine" via Vendor.UserId == the caller's UserId instead of gating by a
/// permission.</summary>
public class VendorPortalService : IVendorPortalService
{
    private readonly IVendorRepository _vendorRepo;
    private readonly IPurchaseOrderRepository _purchaseOrderRepo;
    private readonly IVendorBillRepository _vendorBillRepo;

    public VendorPortalService(IVendorRepository vendorRepo,
        IPurchaseOrderRepository purchaseOrderRepo, IVendorBillRepository vendorBillRepo)
    {
        _vendorRepo = vendorRepo;
        _purchaseOrderRepo = purchaseOrderRepo;
        _vendorBillRepo = vendorBillRepo;
    }

    private async Task<Vendor> GetMyVendorAsync(int orgId, int userId) =>
        await _vendorRepo.GetByUserIdAsync(orgId, userId)
            ?? throw new KeyNotFoundException("No vendor record is linked to your account.");

    public async Task<MyVendorDto> GetMyProfileAsync(int orgId, int userId)
    {
        var vendor = await GetMyVendorAsync(orgId, userId);
        return new MyVendorDto
        {
            Id = vendor.Id,
            Name = vendor.Name,
            Category = vendor.Category,
            ContactEmail = vendor.ContactEmail,
            ContactPhone = vendor.ContactPhone,
            Address = vendor.Address,
            IsActive = vendor.IsActive
        };
    }

    public async Task<List<PurchaseOrderDto>> GetMyPurchaseOrdersAsync(int orgId, int userId)
    {
        var vendor = await GetMyVendorAsync(orgId, userId);
        var orders = await _purchaseOrderRepo.GetByVendorIdAsync(orgId, vendor.Id);
        return orders.Select(ToDto).ToList();
    }

    public async Task<List<VendorBillDto>> GetMyBillsAsync(int orgId, int userId)
    {
        var vendor = await GetMyVendorAsync(orgId, userId);
        var bills = await _vendorBillRepo.GetByVendorIdAsync(orgId, vendor.Id);
        return bills.Select(ToDto).ToList();
    }

    private static PurchaseOrderDto ToDto(PurchaseOrder o)
    {
        var lineDtos = o.Lines.OrderBy(l => l.DisplayOrder).Select(l =>
        {
            var subtotal = l.Quantity * l.UnitPrice;
            var tax = subtotal * l.TaxRatePercent / 100m;
            return new PurchaseOrderLineDto
            {
                Id = l.Id,
                ItemName = l.ItemName,
                ProductId = l.ProductId,
                ProductName = l.Product?.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxCodeId = l.TaxCodeId,
                TaxCodeName = l.TaxCode?.Name,
                TaxRatePercent = l.TaxRatePercent,
                DisplayOrder = l.DisplayOrder,
                LineSubtotal = subtotal,
                LineTax = tax,
                LineTotal = subtotal + tax
            };
        }).ToList();

        return new PurchaseOrderDto
        {
            Id = o.Id,
            PoNumber = o.PoNumber,
            VendorId = o.VendorId,
            VendorName = o.Vendor?.Name ?? string.Empty,
            RfqRequestId = o.RfqRequestId,
            RfqNumber = o.RfqRequest?.RfqNumber,
            Status = o.Status,
            OrderDate = o.OrderDate,
            OwnerId = o.OwnerId,
            OwnerName = o.Owner?.Name ?? string.Empty,
            Lines = lineDtos,
            Subtotal = lineDtos.Sum(l => l.LineSubtotal),
            TaxTotal = lineDtos.Sum(l => l.LineTax),
            GrandTotal = lineDtos.Sum(l => l.LineTotal)
        };
    }

    private static VendorBillDto ToDto(VendorBill b)
    {
        var lineDtos = b.Lines.OrderBy(l => l.DisplayOrder).Select(l => new VendorBillLineDto
        {
            Id = l.Id,
            LedgerAccountId = l.LedgerAccountId,
            LedgerAccountName = l.LedgerAccount?.Name ?? string.Empty,
            LedgerAccountCode = l.LedgerAccount?.Code ?? string.Empty,
            Description = l.Description,
            Amount = l.Amount,
            DisplayOrder = l.DisplayOrder
        }).ToList();

        return new VendorBillDto
        {
            Id = b.Id,
            BillNumber = b.BillNumber,
            VendorId = b.VendorId,
            VendorName = b.Vendor?.Name ?? string.Empty,
            PayableLedgerAccountId = b.PayableLedgerAccountId,
            PayableLedgerAccountName = b.PayableLedgerAccount?.Name ?? string.Empty,
            BillDate = b.BillDate,
            DueDate = b.DueDate,
            Status = b.Status,
            OwnerId = b.OwnerId,
            OwnerName = b.Owner?.Name ?? string.Empty,
            PostedJournalEntryId = b.PostedJournalEntryId,
            PaymentJournalEntryId = b.PaymentJournalEntryId,
            Lines = lineDtos,
            TotalAmount = lineDtos.Sum(l => l.Amount)
        };
    }
}
