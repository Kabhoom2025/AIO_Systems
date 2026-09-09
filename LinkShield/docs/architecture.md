# LinkShield AI — Architecture

## Overview

LinkShield AI analyzes a submitted URL and produces an explainable risk score by
combining several independent analysis signals:

```
Angular (client/) --> .NET 9 API (LinkShield.API)
                          |-- Authentication/Authorization (JWT + refresh, RBAC)
                          |-- URL Analysis
                          |-- Domain Intelligence (RDAP)
                          |-- DNS Analysis
                          |-- SSL/TLS Analysis
                          |-- Redirect Analysis (SSRF-safe)
                          |-- Threat Intelligence (pluggable providers)
                          |-- Brand Impersonation Detection
                          |-- Risk Scoring Engine (configurable weights)
                          |-- Scan History / Audit
                          |-- SignalR (live scan-stage updates)
                          |-- PostgreSQL (LinkShieldDbContext)
                          |-- Redis (cache, dedupe, rate limiting)
                          `-- ml-service (FastAPI, Python) via HttpClientFactory + Polly
LinkShield.Worker  --> background scan processing, TI cache refresh, cleanup, notifications
```

## Layering (Clean Architecture)

- **LinkShield.Domain** — entities, enums, no external dependencies. Source of truth for the
  data model (see `docs/database.md`).
- **LinkShield.Application** — use-case interfaces, DTOs, validators, mapping. Depends only on
  Domain.
- **LinkShield.Infrastructure** — EF Core (`LinkShieldDbContext`), Npgsql, Redis cache, external
  HTTP clients (threat-intel providers, RDAP, DNS, ml-service), Polly resilience policies.
- **LinkShield.API** — controllers, SignalR hub (`ScanProgressHub`), JWT auth, Swagger, health
  checks, Serilog request logging.
- **LinkShield.Worker** — `BackgroundService` implementations for long-running scan processing
  and scheduled threat-intel/reputation refresh, so scans never block the request pipeline.

This mirrors the Repository/Service-over-Clean-Architecture shape already used by
`NovaERP`, `HRMSManagement`, and the other AIO_Systems verticals — no MediatR/CQRS, so
LinkShield stays consistent with its siblings.

## Core scan flow (see spec section 4)

1. Client calls `POST /api/v1/scans` with a raw URL.
2. API normalizes the URL and enqueues a `UrlScan` row (`ScanStatus.Queued`), returns its id
   immediately.
3. `LinkShield.Worker` (or an inline pipeline for low-traffic dev mode) walks the scan through
   each stage, broadcasting `ScanStatus` transitions over `ScanProgressHub` as it goes:
   `UrlAnalysis -> DomainAnalysis -> DnsAnalysis -> SslAnalysis -> RedirectAnalysis ->
   ThreatIntelligence -> BrandDetection -> MlAnalysis -> RiskScoring -> Completed`.
4. Every stage persists its own result row (`UrlAnalysis`, `DomainAnalysis`, `DnsAnalysis`,
   `SslAnalysis`, `RedirectAnalysis`, `ThreatIntelligenceResult`, `BrandMatch`, `MlPrediction`).
5. The risk engine combines all signals into a `RiskFactor` breakdown, a 0-100 `RiskScore`, a
   `RiskLevel`, and a `ScanVerdict` — every point on the score traces back to a specific
   `RiskFactor` row for explainability (spec section 13).

## SSRF protection boundary

Every outbound network call the platform makes on behalf of a user-submitted URL (DNS
resolution, HTTP fetch, redirect following, RDAP/cert lookups) must go through the same
validation gate: resolve the hostname, reject private/loopback/link-local/multicast ranges and
cloud metadata addresses, enforce scheme/timeout/redirect-count/response-size limits, and
re-validate on every redirect hop. This lives in `LinkShield.Infrastructure` behind a single
`ISsrfSafeHttpClientFactory`-style abstraction so no analysis component can accidentally bypass
it — see `docs/security.md`.

## Extensibility points

- **Threat-intel providers** — `IThreatIntelligenceProvider`, one adapter per provider,
  registered against `ThreatIntelligenceProvider` config rows. Adding a provider never touches
  the risk engine.
- **Risk rules** — `RiskRule` rows are data, not code; weights/conditions are admin-editable.
- **ML models** — `ml-service` versions its artifact (`MlPrediction.ModelVersion`); the .NET side
  only ever calls `POST /api/v1/predict` and stores whatever version comes back.
- **Brand profiles** — `BrandProfile` rows are data; typosquat/homoglyph detection reads from
  the table, not a hardcoded list.

## Status

This is the initial scaffold: Domain entities, EF Core schema/migration, and a bootable
`LinkShield.API` (health checks, Swagger, JWT wiring, SignalR hub, Serilog) exist and build.
Business logic for each analysis stage is not implemented yet — see `PROJECT_PLAN.md` for the
build order.
