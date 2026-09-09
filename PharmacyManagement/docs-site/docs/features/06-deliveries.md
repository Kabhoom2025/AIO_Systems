---
sidebar_position: 6
---

# Deliveries

Tracking home/local delivery of orders after a sale.

## What it does

![Deliveries](/img/screenshots/06-deliveries.png)
*The Deliveries queue, filterable by status.*

- Deliveries are scheduled against a sale, with a status lifecycle (Scheduled → Assigned → Out for Delivery → Delivered, plus Cancelled) and an assignable delivery-staff user.
- The Deliveries list is filterable by status, giving dispatch staff a working queue.
- A dedicated `Delivery Staff` role exists in the seeded role set, scoped to only what a delivery person needs to see (their assigned deliveries, updating status) — not full sales or inventory access.

## Key files

- Backend: `Pharmacy.Domain/Entities/Delivery.cs`, `Pharmacy.API/Controllers/DeliveryController.cs`.
- Frontend: `client/src/app/features/deliveries/`.

## Permissions

Gated by `deliveries.view`/`deliveries.edit` — a delivery-staff role typically gets view + edit (to update status) but not create/delete, since deliveries are created as a byproduct of a sale, not independently.
