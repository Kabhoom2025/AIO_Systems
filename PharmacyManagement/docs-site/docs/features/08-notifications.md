---
sidebar_position: 8
---

# Notifications

Automatic in-app alerts for the two conditions a pharmacist most needs to know about proactively: running low on stock, and stock that's about to expire.

## What it does

![Notification bell dropdown](/img/screenshots/08-notifications.png)
*The bell-icon inbox showing a low-stock alert and an expiring-batch alert.*

- **Low-stock alerts** — generated when a medicine's total stock (summed across its batches) drops to or below its configured reorder level.
- **Expiring-batch alerts** — generated for any batch within 30 days of its expiry date.
- **Bell icon inbox** — a toolbar bell shows an unread-count badge and opens a dropdown listing recent notifications (unread first), with "mark read" per item and "mark all read."
- **Idempotent generation** — alerts are deduplicated per `(organization, type, related entity, calendar day)`, so the same condition doesn't spam a new notification every time it's checked — a still-low-stock item gets exactly one fresh notification per day, not one per page load.

As of [Real-Time Push](./signalr), generation also runs proactively on a server-side timer and pushes new alerts to connected clients instantly, rather than only being computed when a client happens to ask for the notification list.

## Key files

- Backend: `Pharmacy.Domain/Entities/Notification.cs`, `Pharmacy.Application/Services/NotificationService.cs`, `Pharmacy.API/Controllers/NotificationController.cs`.
- Frontend: `client/src/app/core/notifications-api.service.ts`, bell UI in `client/src/app/pharmacy-shell/`.

## Scope

Only two alert types are wired up (low-stock, expiring-batch), but the `Notification` schema (`Type`, `Title`, `Message`, `RelatedEntityType`/`RelatedEntityId`) is generic enough that a later addition (e.g. delivery-status changes, PO approvals) can create rows through the same service without a schema change.
