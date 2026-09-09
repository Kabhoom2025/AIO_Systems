# LinkShield AI — Database

PostgreSQL, EF Core Code First, `LinkShieldDbContext`
(`LinkShield.Infrastructure/Persistence/LinkShieldDbContext.cs`). All entities inherit
`BaseEntity` (`Id: Guid`, `CreatedAtUtc`, `UpdatedAtUtc`, `IsDeleted` + soft-delete query filter,
`RowVersion` optimistic-concurrency token).

The initial migration is generated and checked in at
`LinkShield.Infrastructure/Persistence/Migrations/20260811181426_InitialCreate.cs` — run it with:

```powershell
cd LinkShield\LinkShield.Infrastructure
dotnet ef database update --startup-project ..\LinkShield.API\LinkShield.API.csproj
```

## Tables

| Table | Purpose |
|---|---|
| `Users`, `Roles`, `UserRoles`, `RefreshTokens` | Auth + RBAC (`SuperAdmin`, `Admin`, `SecurityAnalyst`, `User`, `ApiClient`) |
| `ApiClients`, `ApiKeys` | Enterprise API consumers, quota/rate-limit config per client |
| `UrlScans` | One row per scan request — status, score, level, verdict, confidence, correlation id |
| `UrlAnalyses` | Structural URL signals (length, encoding, punycode, keywords, suspicious TLD, etc.) |
| `DomainAnalyses` | RDAP-derived domain/registrar/age/nameserver data |
| `DnsAnalyses` | A/AAAA/MX/NS/CNAME/TXT records, resolves flag, suspicious-nameserver flag |
| `SslAnalyses` | Certificate validity, issuer, SAN, domain match, TLS version |
| `RedirectAnalyses` | One row per redirect hop — from/to URL, domain/HTTPS-downgrade flags |
| `ThreatIntelligenceProviders`, `ThreatIntelligenceResults` | Provider registry + per-scan, per-provider query results (cached) |
| `BrandProfiles`, `BrandMatches` | Configurable brand list + per-scan typosquat/homoglyph matches |
| `RiskRules`, `RiskFactors` | Admin-editable scoring rules + the per-scan factors that actually fired |
| `MlPredictions` | ml-service response per scan (prediction, probability, model version) |
| `ScanEvents` | Stage-transition timeline per scan (mirrors the SignalR broadcast) |
| `AuditLogs` | Security-relevant actions (login, role change, risk-rule change, API key lifecycle, ...) |
| `Notifications` | In-app/email/SignalR notifications per user |
| `SystemSettings` | Key/value config, including risk-engine thresholds and category weights |

## Relationships worth knowing

- `UrlScan` is the aggregate root for a scan: 1:1 with `UrlAnalysis`/`DomainAnalysis`/
  `DnsAnalysis`/`SslAnalysis`/`MlPrediction`, 1:many with `RedirectAnalysis`/
  `ThreatIntelligenceResult`/`BrandMatch`/`RiskFactor`/`ScanEvent`. Deleting a scan cascades to
  all of these.
- `ThreatIntelligenceResult.ThreatIntelligenceProviderId` is `Restrict` on delete — a provider
  can't be removed while historical results reference it (deactivate via `IsEnabled` instead).
- `RiskFactor.RiskRuleId` is nullable + `SetNull` — a factor survives even if the rule that
  produced it is later deleted, preserving explainability of past scans.

## Seed data

`LinkShield.Infrastructure/Persistence/SeedData.cs` seeds reference data every environment
needs: the five system roles, the four threat-intel provider registrations (Google Safe
Browsing, VirusTotal, URLhaus, PhishTank — API keys left blank, configured via
`appsettings`/secrets), five starter brand profiles, eight starter risk rules matching the
weights in spec section 12, and the risk-engine threshold/weight `SystemSettings` rows. This is
not demo data — it's what the risk engine and threat-intel abstraction need to be functional on
first run.
