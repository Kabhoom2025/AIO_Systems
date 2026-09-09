---
sidebar_position: 15
---

# Audit Logging

The final roadmap phase: a tamper-evident record of who did what, across the entire application, added with zero changes to any existing controller or service.

## What it does

![Audit Log](/img/screenshots/15-audit-log.png)
*The Audit Log — every mutating action across the app, with color-coded Create/Update/Delete badges.*

- **One global filter, every controller** — `AuditLogActionFilter` is registered once as a global MVC action filter. It transparently records every successful mutating HTTP request (POST/PUT/PATCH/DELETE) across all 20+ controllers, with no per-feature logging calls needed anywhere.
- **What gets recorded** — timestamp, the acting user's id and name (from JWT claims already present on every request), the organization, the HTTP method mapped to a human action (`Create`/`Update`/`Delete`), the entity type (derived from the controller name), and the entity id (from route data, when the route follows the dominant `{id}` convention).
- **No body capture, by design** — the filter deliberately never reads the request body, so nothing sensitive (like a change-password payload) can ever be logged. This means the audit trail records *that* an action happened, not a field-level before/after diff.
- **Audit Log viewer** — a simple, most-recent-first table (Timestamp, User, Action, Entity, Entity Id), gated by a new `audit-logs.view` permission.

## Key files

- Backend: `Pharmacy.Domain/Entities/AuditLog.cs`, `Pharmacy.API/Filters/AuditLogActionFilter.cs`, `Pharmacy.Infrastructure/Services/{AuditLogWriter,AuditLogService}.cs`, `Pharmacy.API/Controllers/AuditLogController.cs`.
- Frontend: `client/src/app/features/audit-logs/`.

## How it was verified

A handful of real actions were performed through the UI across different modules (editing a medicine, creating a supplier, creating then deleting an expense) alongside some GET-only browsing. The Audit Log page showed exactly the four mutating actions — correctly attributed, correctly typed — and zero entries from the GET-only browsing, confirming the filter's read/write distinction works as designed.

## Scope

Records *that* something changed, not a full before/after diff of every field — true field-level history would need EF Core change-tracker interception, a materially larger feature. No export or retention policy on the log itself yet (though it could reuse the CSV export utility from an earlier phase). GET requests are never logged — only mutations carry audit value.
