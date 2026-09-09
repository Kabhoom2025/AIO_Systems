using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

/// <summary>Self-service orchestration for a logged-in User's own Contact record — no crm.view
/// permission is checked anywhere here, mirroring EmployeePortalService's reasoning: crm.view
/// is Admin/Manager-facing, and an external customer has no crm.* permission at all, so this
/// service scopes every query to "mine" via Contact.UserId == the caller's UserId instead of
/// gating by a permission. A Contact not linked to an Account has no orders/invoices to show —
/// both list endpoints simply return empty in that case, matching a customer with no purchase
/// history yet rather than treating it as an error.</summary>
public class CustomerPortalService : ICustomerPortalService
{
    private readonly IContactRepository _contactRepo;
    private readonly ISalesOrderRepository _salesOrderRepo;
    private readonly ICustomerInvoiceRepository _customerInvoiceRepo;

    public CustomerPortalService(IContactRepository contactRepo,
        ISalesOrderRepository salesOrderRepo, ICustomerInvoiceRepository customerInvoiceRepo)
    {
        _contactRepo = contactRepo;
        _salesOrderRepo = salesOrderRepo;
        _customerInvoiceRepo = customerInvoiceRepo;
    }

    private async Task<Contact> GetMyContactAsync(int orgId, int userId) =>
        await _contactRepo.GetByUserIdAsync(orgId, userId)
            ?? throw new KeyNotFoundException("No customer record is linked to your account.");

    public async Task<MyContactDto> GetMyProfileAsync(int orgId, int userId)
    {
        var contact = await GetMyContactAsync(orgId, userId);
        return new MyContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Title = contact.Title,
            AccountId = contact.AccountId,
            AccountName = contact.Account?.Name
        };
    }

    public async Task<List<SalesOrderDto>> GetMyOrdersAsync(int orgId, int userId)
    {
        var contact = await GetMyContactAsync(orgId, userId);
        if (contact.AccountId is null) return new List<SalesOrderDto>();

        var orders = await _salesOrderRepo.GetByAccountIdAsync(orgId, contact.AccountId.Value);
        return orders.Select(ToDto).ToList();
    }

    public async Task<List<CustomerInvoiceDto>> GetMyInvoicesAsync(int orgId, int userId)
    {
        var contact = await GetMyContactAsync(orgId, userId);
        if (contact.AccountId is null) return new List<CustomerInvoiceDto>();

        var invoices = await _customerInvoiceRepo.GetByAccountIdAsync(orgId, contact.AccountId.Value);
        return invoices.Select(ToDto).ToList();
    }

    private static SalesOrderDto ToDto(SalesOrder o)
    {
        var lineDtos = o.Lines.OrderBy(l => l.DisplayOrder).Select(l =>
        {
            var subtotal = l.Quantity * l.UnitPrice;
            var tax = subtotal * l.TaxRatePercent / 100m;
            return new SalesOrderLineDto
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

        return new SalesOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            AccountId = o.AccountId,
            AccountName = o.Account?.Name ?? string.Empty,
            OpportunityId = o.OpportunityId,
            OpportunityName = o.Opportunity?.Name,
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

    private static CustomerInvoiceDto ToDto(CustomerInvoice i)
    {
        var lineDtos = i.Lines.OrderBy(l => l.DisplayOrder).Select(l => new CustomerInvoiceLineDto
        {
            Id = l.Id,
            LedgerAccountId = l.LedgerAccountId,
            LedgerAccountName = l.LedgerAccount?.Name ?? string.Empty,
            LedgerAccountCode = l.LedgerAccount?.Code ?? string.Empty,
            Description = l.Description,
            Amount = l.Amount,
            DisplayOrder = l.DisplayOrder
        }).ToList();

        return new CustomerInvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            AccountId = i.AccountId,
            AccountName = i.Account?.Name ?? string.Empty,
            ReceivableLedgerAccountId = i.ReceivableLedgerAccountId,
            ReceivableLedgerAccountName = i.ReceivableLedgerAccount?.Name ?? string.Empty,
            InvoiceDate = i.InvoiceDate,
            DueDate = i.DueDate,
            Status = i.Status,
            OwnerId = i.OwnerId,
            OwnerName = i.Owner?.Name ?? string.Empty,
            PostedJournalEntryId = i.PostedJournalEntryId,
            PaymentJournalEntryId = i.PaymentJournalEntryId,
            Lines = lineDtos,
            TotalAmount = lineDtos.Sum(l => l.Amount)
        };
    }
}
