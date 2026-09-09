---
sidebar_position: 11
---

# AI Insights

Rule-based, data-driven recommendations — explicitly *not* a generative-AI/LLM feature, since no such infrastructure (API keys, SDKs) exists in this system.

## What it does

![AI Insights page](/img/screenshots/11-ai-insights.png)
*Smart Reorder Suggestions and Expiry Risk, both computed from real sales/stock data.*

- **Smart Reorder Suggestions** — for each medicine, computes recent sales velocity (average daily units sold over the last 30 days) and compares it against current stock. Medicines that are already at/below their reorder level, or projected to run out within 14 days at current sales pace, get a suggested reorder quantity — calculated from velocity where there's sales history, or a simple stock-doubling heuristic where there isn't.
- **Expiry Risk Scoring** — combines the same sales-velocity data with existing expiry-alert data to flag batches as `High` risk (won't sell through before they expire, at current pace), `Medium` (expiring soon, insufficient sales data to project), or `Low`.

## Key files

- Backend: `Pharmacy.Application/DTOs/AiInsightsDtos.cs`, `Pharmacy.Infrastructure/Services/AiInsightsService.cs`, `Pharmacy.API/Controllers/AiInsightsController.cs`.
- Frontend: `client/src/app/features/ai-insights/`.

## Why "rule-based" and not an LLM

When this phase was scoped, the choice was explicitly offered and the rule-based approach was chosen over integrating a real LLM (which would require an API key, ongoing cost, and a network dependency). The two insight types here reuse the exact same sales-velocity computation, both read from data the app already collects (Sales, SaleItems, Medicine batches), and are entirely self-contained — no external API calls, no cost, works fully offline. Both endpoints reuse the existing `reports.view` permission rather than introducing a new one.
