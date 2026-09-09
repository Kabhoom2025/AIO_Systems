---
sidebar_position: 10
---

# Import / Export / Print

Generalizing the app's printing pattern to more documents, and adding the first data import/export capability.

## What it does

![Export/Import buttons](/img/screenshots/10-medicines-export-import.png)
*Export CSV / Import CSV buttons added to the Medicines page header.*

![Printable Purchase Order](/img/screenshots/10-po-print-dialog.png)
*The Purchase Order detail dialog with its new "Print PO" button.*

- **CSV export** — a one-line-per-page `exportToCsv()` utility, wired into Medicines, Suppliers, Purchase Orders, Sales History, and Expenses. Builds a CSV client-side (no backend involvement — it exports whatever's already loaded into the page) and triggers a browser download via a `Blob`.
- **CSV import** — Medicines gained an "Import CSV" button that bulk-creates medicines by looping the existing single-row create endpoint over parsed CSV rows, reusing its existing SKU/barcode auto-generation. Shows a success/failure count when done.
- **Printable Purchase Order** — the existing PO detail dialog gained a "Print PO" button, reusing the same dialog-overlay + `no-print` + `@media print` pattern established for the POS receipt and medicine labels.
- **Printable invoice reprint** — Sales History's detail dialog similarly gained a "Print" button for reprinting a past invoice.

## Key files

- Frontend: `client/src/app/core/csv.util.ts` (the export/parse utility), plus the export/import/print additions inside each feature's existing component files (`medicines`, `suppliers`, `purchase-orders`, `sales-history`, `expenses`).

## Scope

CSV only — no Excel (`.xlsx`) or PDF export, to avoid vetting more packages against the Native Federation shared-package risk found in the previous phase, for marginal benefit over a universally-openable CSV. Import is scoped to Medicines only, the one entity with an obvious bulk-seeding use case and an existing single-row create endpoint a client-side loop can drive safely.
