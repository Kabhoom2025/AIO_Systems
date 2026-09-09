---
sidebar_position: 4
---

# Point of Sale & Sales

The checkout workflow and the resulting sales history.

## What it does

![Point of Sale](/img/screenshots/04-pos.png)
*The POS cart — search or scan a medicine, adjust quantity/discount per line, checkout.*

![Receipt](/img/screenshots/04-pos-receipt.png)
*Post-checkout receipt, printable via the browser's print dialog.*

![Sales History](/img/screenshots/04-sales-history.png)
*Sales History — every past invoice, with a detail/reprint view.*

- **Cart-based POS** — search or scan a medicine, add it to a cart with quantity/discount editable per line, pick an optional customer and a payment method, and check out.
- **FEFO stock consumption** — `SaleService` consumes stock from the batch with the earliest expiry date first, across as many batches as needed to fulfill the line quantity, rather than an arbitrary batch.
- **Invoice generation** — checkout creates a `Sale` with auto-generated invoice number, subtotal/discount/tax/total, and one `SaleItem` per cart line (recording which batch(es) were drawn from).
- **Printable receipt** — the post-checkout confirmation dialog can be printed via the browser's native print dialog (`window.print()` + an `@media print` stylesheet that hides everything except the receipt).
- **Sales History** — a searchable list of past sales with a detail view per invoice, later extended (see [Import/Export/Print](./import-export-print)) with CSV export and invoice reprinting.

## Key files

- Backend: `Pharmacy.Domain/Entities/{Sale,SaleItem}.cs`, `Pharmacy.Application/Services/SaleService.cs`, `Pharmacy.API/Controllers/SaleController.cs`.
- Frontend: `client/src/app/features/{pos,sales-history}/`.

## Permissions

`sales.create` is required to use POS (making a sale); `sales.view` is required to see Sales History. These are deliberately separate — a cashier role can be granted create-only.
