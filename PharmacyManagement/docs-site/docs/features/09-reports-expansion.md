---
sidebar_position: 9
---

# Reports Suite Expansion

Operational reporting layered on top of the original financial reports, plus the app's first chart.

## What it does

![Sales & Inventory reports](/img/screenshots/09-reports-operational.png)
*The Sales & Inventory tab — sales trend chart, top-selling medicines, and inventory valuation.*

- **Sales Summary** — total sales, transaction count, and average ticket for a date range, plus a daily sales-trend line chart and a payment-method breakdown table.
- **Top-Selling Medicines** — ranks medicines by quantity sold within a date range.
- **Inventory Valuation** — a live snapshot of every medicine's current stock × purchase price, with a grand total — this one has no date filter since it reflects right-now stock, not historical activity.
- **Tabbed Reports page** — the original Financial reports (P&L, GST, Cash Book) moved into a "Financial" tab, with the new operational reports living in a "Sales & Inventory" tab.

## Key files

- Backend: extended `Pharmacy.Infrastructure/Services/ReportService.cs` and `Pharmacy.API/Controllers/ReportController.cs` with `sales-summary`, `top-medicines`, and `inventory-valuation` endpoints.
- Frontend: `client/src/app/features/reports/`.

## A Native Federation lesson from this phase

The first chart implementation used PrimeNG's `p-chart` wrapper component and crashed the whole federated app at runtime with "Unable to resolve specifier 'primeng/chart'" — even after adding it to the federation config's shared-package skip list. The root cause, found by reading Native Federation's source: `shareAll()`'s auto-discovery of a shared package's *secondary entry points* (like `primeng/chart`) is filtered against the library's own internal default skip list, not the app's custom skip list — so it silently slips back into the shared registry no matter what the app config says. The fix has two parts: `federation.config.js` now explicitly strips any matching key out of the computed `shared` object after `shareAll()` runs (a defensive fix that protects future phases too), and the chart itself was switched from `p-chart` to plain `chart.js` via a `<canvas>` ref — sidestepping the whole class of risk, since a plain npm package has no ambiguous secondary-export surface for Native Federation to mishandle.
