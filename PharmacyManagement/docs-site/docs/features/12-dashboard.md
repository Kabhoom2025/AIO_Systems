---
sidebar_position: 12
---

# Customizable Dashboard

The new landing page after login — a widget-based overview built entirely from data other features already expose.

## What it does

![Dashboard](/img/screenshots/12-dashboard.png)
*The Dashboard landing page with all six widgets.*

![Customize mode](/img/screenshots/12-dashboard-customize.png)
*Customize mode — drag handles and hide buttons appear on each widget.*

- **Six widgets** — Today's Sales, Inventory Valuation, Unread Notifications, Top-Selling Medicines, Reorder Suggestions, and Expiry Risk — each backed by an existing endpoint from Reports, Notifications, or AI Insights. No new backend endpoints were needed for this phase.
- **Drag-to-reorder** — a "Customize" mode (via Angular CDK drag-and-drop) lets a user reorder widgets by dragging a handle.
- **Show/hide** — each widget can be hidden in Customize mode; hidden widgets appear in a dimmed "Hidden — click to restore" strip rather than disappearing without a trace.
- **Persisted layout** — the chosen order and hidden set are saved to `localStorage` (`pharmacy_dashboard_layout`), following the same signal + localStorage convention as the existing dark/light theme preference, so the layout survives a reload.

## Key files

- Frontend only: `client/src/app/features/dashboard/`.

## Scope

Six fixed widget types, not a widget-authoring framework — "customizable" here means show/hide and reorder of a known set, not user-defined custom widgets. Layout persistence is per-browser (`localStorage`), not synced server-side per user account.
