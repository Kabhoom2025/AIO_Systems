---
sidebar_position: 1
---

# Inventory, Suppliers & Purchasing

The foundation module: medicine catalog, batch-level stock tracking, suppliers, and the purchase-to-receipt workflow.

## What it does

![Medicines list](/img/screenshots/01-medicines.png)
*The Medicines list — category, pack details, MRP, live stock, and auto-generated SKU/barcode per medicine.*

![Suppliers](/img/screenshots/01-suppliers.png)
*Supplier directory.*

![Purchase Orders](/img/screenshots/01-purchase-orders.png)
*Purchase Orders list with status badges.*

- **Medicines** — create/edit/deactivate medicines with category, drug schedule (OTC/H/H1/X), pack size, unit, GST rate, MRP, purchase price, reorder level, and rack location.
- **Batches** — each medicine can have multiple batches with their own expiry date, manufacturing date, quantity, and purchase price. Stock is always computed as the sum of a medicine's active batch quantities — never a single flat "stock" number — so FEFO (first-expiry-first-out) consumption is possible at the sale/adjustment level.
- **Suppliers** — supplier directory with contact info, GST number, payment terms, and credit limit.
- **Purchase Orders** — draft → sent → partially received → received → cancelled lifecycle, with line items per medicine.
- **Goods Receipts** — recording stock actually received against a PO, which creates new medicine batches and updates the PO's received quantities.
- **Stock Adjustments** — manual increase/decrease entries with a reason, for damage, loss, or corrections outside the normal purchase flow.
- **Expiry Alerts** — a dedicated page listing batches expiring within a configurable window (30/60/90 days), color-coded by urgency.

## Key files

- Backend: `Pharmacy.Domain/Entities/{Medicine,MedicineBatch,Supplier,PurchaseOrder,GoodsReceipt,StockAdjustment}.cs`, matching controllers under `Pharmacy.API/Controllers/`.
- Frontend: `client/src/app/features/{medicines,suppliers,purchase-orders,goods-receipts,stock-adjustments,expiry-alerts}/`.

## Permissions

Each area has its own permission module (`medicines.*`, `suppliers.*`, `purchase-orders.*`, `goods-receipts.*`, `stock-adjustments.*`), following the `{module}.{view|create|edit|delete}` convention. Purchase Orders additionally has a one-off `purchase-orders.approve` permission for sending a draft PO.
