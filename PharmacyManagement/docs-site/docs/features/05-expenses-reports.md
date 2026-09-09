---
sidebar_position: 5
---

# Expenses & Financial Reports

Operating expense tracking and the three core financial reports every pharmacy needs.

## What it does

![Expenses](/img/screenshots/05-expenses.png)
*Expense tracking by category, payment method, and branch.*

![Financial Reports](/img/screenshots/05-reports-financial.png)
*The Financial tab: Profit & Loss, GST Summary, and Cash Book for a chosen date range.*

- **Expenses** — categorized expense entries (Rent, Utilities, Salaries, Marketing, Maintenance, Other) with amount, date, payment method, and branch.
- **Profit & Loss** — revenue (from sales) minus cost of goods sold (batch purchase price × quantity sold) minus expenses, for a chosen date range.
- **GST Summary** — output tax collected on sales minus input tax paid on goods receipts, for the same date range.
- **Cash Book** — a running-balance ledger of all cash-method sales (in) and cash-method expenses (out), sorted chronologically.

## Key files

- Backend: `Pharmacy.Domain/Entities/Expense.cs`, `Pharmacy.Infrastructure/Services/ReportService.cs`, `Pharmacy.API/Controllers/{ExpenseController,ReportController}.cs`.
- Frontend: `client/src/app/features/{expenses,reports}/` — Reports later grew a second tab (see [Reports Suite Expansion](./reports-expansion)).

## A recurring bug class this module surfaced

Reports' date-range inputs default to "this month," computed client-side. An early implementation used `new Date().toISOString().substring(0,10)`, which silently shifts the date backward by a day in any timezone with a positive UTC offset (like India, UTC+5:30) because `toISOString()` always converts to UTC first. The fix — used everywhere a "today" or "first of month" default is needed — builds the date string from `getFullYear()`/`getMonth()`/`getDate()` (local components) instead of going through UTC at all.
