---
sidebar_position: 1
slug: /
---

# Pharmacy ERP — Overview

Pharmacy ERP is a multi-module pharmacy management system built as a .NET 9 Web API (Clean Architecture) backend with an Angular 19 federated micro-frontend client. It covers inventory, purchasing, point of sale, clinical records, financial reporting, real-time notifications, and administrative tooling for running a retail pharmacy.

![Login screen](/img/screenshots/00-login.png)

![Dashboard](/img/screenshots/12-dashboard.png)

## Architecture at a glance

- **Backend** — `Pharmacy.Domain` / `Pharmacy.Application` / `Pharmacy.Infrastructure` / `Pharmacy.API`, EF Core 9 + PostgreSQL, JWT authentication with a dynamic, database-driven permission system.
- **Frontend** — Angular 19 standalone components and signals, loaded as a Native Federation remote (`pharmacy`), Angular Material (M3 theming) + PrimeNG for UI.
- **Real-time** — SignalR pushes notification updates to connected clients instantly instead of relying purely on polling.
- **Cross-cutting** — a global permission catalog drives both server-side authorization policies and client-side UI gating from a single source of truth, and a global audit-log filter records every mutating action across the whole API with no per-controller code.

## Feature modules

Each module below was built as its own scoped phase, with explicit, documented boundaries on what was included and what was deliberately deferred. Use the sidebar to browse each one:

1. **Inventory, Suppliers & Purchasing** — medicines, batches, suppliers, purchase orders, goods receipts, stock adjustments.
2. **App Shell, Theming & RBAC** — the Angular shell, dark/light theme, and the permission-driven navigation and access control.
3. **Clinical Records** — doctors, patients, customers, prescriptions, and expiry tracking.
4. **Point of Sale & Sales** — cart-based checkout, invoicing, printable receipts, sales history.
5. **Expenses & Financial Reports** — expense tracking, Profit & Loss, GST summary, cash book.
6. **Deliveries** — delivery scheduling and status tracking.
7. **Barcode / QR Codes** — label generation/printing and barcode-scanner integration in POS.
8. **Notifications** — automatic low-stock and expiring-batch alerts with a bell-icon inbox.
9. **Reports Suite Expansion** — sales trend charts, top-selling medicines, inventory valuation.
10. **Import / Export / Print** — CSV export across list pages, bulk medicine import, printable purchase orders and invoices.
11. **AI Insights** — rule-based smart reorder suggestions and expiry-risk scoring.
12. **Customizable Dashboard** — a drag-and-drop, show/hide-able widget dashboard.
13. **Settings** — organization profile management and self-service password changes.
14. **Real-Time Push (SignalR)** — instant notification delivery instead of pure polling.
15. **Audit Logging** — a tamper-evident record of who changed what, when.

## Where to find things

- Backend controllers live under `Pharmacy.API/Controllers/`, one per feature area.
- Frontend feature pages live under `client/src/app/features/<feature-name>/`.
- The Angular shell's navigation menu (`pharmacy-shell.component.ts`) is the single map of every routed feature and the permission that gates it.
