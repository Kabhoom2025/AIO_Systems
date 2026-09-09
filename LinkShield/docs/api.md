# LinkShield AI — API

Base URL (dev): `http://localhost:5008` (see `docs/deployment.md` for the full port table).
Swagger UI is available at `/swagger` in Development.

## Implemented: Authentication (`AuthController`)

```
POST /api/v1/auth/register         { email, password, firstName, lastName } -> AuthResponseDto
POST /api/v1/auth/login            { email, password }                     -> AuthResponseDto
POST /api/v1/auth/refresh          { refreshToken }                        -> AuthResponseDto (rotates)
POST /api/v1/auth/logout           { refreshToken }                        -> 204
POST /api/v1/auth/forgot-password  { email }                               -> 204 (always, no enumeration)
POST /api/v1/auth/reset-password   { token, newPassword }                  -> 204
POST /api/v1/auth/verify-email     { token }                               -> 204
POST /api/v1/auth/change-password  { currentPassword, newPassword }        -> 204 (requires JWT bearer)
```

`AuthResponseDto`: `{ accessToken, refreshToken, expiresAtUtc, userId, email, firstName, lastName, roles[] }`.
Every registered user gets the `User` role by default. Passwords are PBKDF2-SHA256 hashed
(100k iterations); refresh tokens rotate on every use (the old one is revoked, reuse returns
401). Five failed logins locks the account for 15 minutes. `forgot-password` never reveals
whether an email exists. `register`/`login`/`forgot-password` are rate-limited to 10
requests/minute per client IP (`429` beyond that) — confirmed live that the 11th attempt in a
minute is throttled. See `docs/security.md` for the full auth security model and
`PROJECT_PLAN.md` for what's deliberately not done yet (MFA, real SMTP —
`LoggingEmailSender` just logs verification/reset tokens for now).

## Implemented: URL analysis & scans (`UrlController`, `ScansController`)

```
POST   /api/v1/url/analyze   { url } -> UrlAnalysisResultDto   — analysis only, no persistence
POST   /api/v1/scans         { url } -> ScanDetailDto, 201 Created + Location header
GET    /api/v1/scans/{id}    -> ScanDetailDto, 404 if not found
GET    /api/v1/scans?page=&pageSize= -> PagedResultDto<ScanSummaryDto>
DELETE /api/v1/scans/{id}    -> 204 (soft delete), 404 if not found
```

`POST /api/v1/scans` is `[AllowAnonymous]` (a public scanning tool doesn't require login) but
still attaches `UserId` when a valid JWT bearer is present. It runs the URL-analysis stage
synchronously (fast, pure computation) and returns immediately with that much of the result;
every other stage (domain/DNS/SSL/redirect/threat-intelligence/brand-detection/ML/risk-scoring)
then runs in the background — see `PROJECT_PLAN.md`'s pipeline writeup — with `ScanProgressHub`
broadcasting each stage transition. Poll `GET /api/v1/scans/{id}` to watch `status` advance from
`UrlAnalysis` through to `Completed` (or `Failed`, with `failureReason` set).

`UrlAnalysisResultDto` carries every spec-section-5 signal — see `PROJECT_PLAN.md` for the full
field list and known limitations (heuristic homoglyph detection, built-in multi-part-TLD list
instead of a full public suffix list).

## Implemented: full analysis pipeline, threat intelligence, risk engine

Not separate REST endpoints — these run automatically as part of the `POST /api/v1/scans`
background pipeline and their results appear in `GET /api/v1/scans/{id}`'s `domainAnalysis`,
`dnsAnalysis`, `sslAnalysis`, `redirectAnalysis`, `threatIntelligenceResults`, `brandMatches`,
`mlPrediction`, and `riskFactors` fields once each stage completes. See `PROJECT_PLAN.md` for
what each analyzer actually does and how the risk engine combines them.

## Implemented: Dashboard (`DashboardController`)

```
GET /api/v1/dashboard/summary        -> { totalScans, safeCount, suspiciousCount, maliciousCount,
                                           criticalCount, threatsDetected, averageRiskScore,
                                           threatIntelligenceMatches }
GET /api/v1/dashboard/trends?days=14 -> { scansOverTime[], riskDistribution[], threatCategories[],
                                           topSuspiciousDomains[], topTargetedBrands[] }
```

`topSuspiciousDomains` is keyed off `RiskScore > 0`, not `Verdict` — a scan can carry real signal
while still reporting `Verdict=Unknown` (e.g. ml-service unreachable lowering confidence), and
that's still worth surfacing on a dashboard. Backs the Angular dashboard's stat tiles and charts
— see `LinkShield/client/src/app/features/dashboard`.

## Implemented: Admin panel (`BrandsController`, `RiskRulesController`, `AuditLogsController`)

All three require `[Authorize(Roles = "SuperAdmin,Admin")]` — confirmed live: 401 unauthenticated,
403 for a `User`-role account, 200 for an admin. A demo `SuperAdmin` account is seeded:
`admin@linkshield.local` / `Admin@123`.

```
GET  /api/v1/brands                -> BrandProfileDto[]
POST /api/v1/brands                { brandName, officialDomain, aliasDomains? } -> BrandProfileDto, 409 on duplicate domain

GET  /api/v1/risk-rules            -> RiskRuleDto[]
POST /api/v1/risk-rules            { name, description, category, weight, conditionExpression, scoreContribution } -> RiskRuleDto
PUT  /api/v1/risk-rules/{id}       { name, description, weight, isEnabled, conditionExpression, scoreContribution } -> RiskRuleDto

GET  /api/v1/audit-logs?page=&pageSize= -> PagedResultDto<AuditLogDto>
```

Every brand/risk-rule create or update writes a real `AuditLog` row
(`BrandProfileChanged`/`RiskRuleChanged`), and `AuthService`/`ScanService` also write
`Register`/`Login`/`Logout`/`PasswordReset`/`ScanCreated`/`ScanDeleted` entries — the audit log
is populated by real application events, not a placeholder table. Backs the Angular admin panel
at `/admin` (`LinkShield/client/src/app/features/admin`), route-guarded by `adminGuard`.

## Implemented: API-key management (`ApiClientsController`) — admin only

```
GET  /api/v1/api-clients                 -> ApiClientDto[] (includes each client's keys)
POST /api/v1/api-clients                 { organizationName, contactEmail, dailyQuota?, requestsPerMinute? } -> ApiClientDto
POST /api/v1/api-clients/{id}/keys       { label, expiresAtUtc? } -> CreatedApiKeyDto { rawKey, ... }
DELETE /api/v1/api-clients/keys/{keyId}  -> 204 (revokes)
```

`CreatedApiKeyDto.rawKey` is returned exactly once, at creation — only its SHA-256 hash is ever
stored (`ApiKeyHasher`), so there is no "view key again" endpoint by design.

## Implemented: Enterprise API (`ExternalController`) — X-API-Key auth, not JWT

```
POST /api/v1/external/url/check   { url } -> ScanDetailDto   (header: X-API-Key: ls_live_...)
```

Authenticated by a separate ASP.NET Core scheme (`ApiKeyAuthenticationHandler`, header
`X-API-Key`) rather than the JWT bearer scheme everything else uses. Runs the same scan pipeline
as `POST /api/v1/scans`, attributing the scan to the API client instead of a user. Enforces both
the client's daily quota (`ApiClient.DailyQuota`, counted from that client's `UrlScans` since
midnight UTC — confirmed live that request 6 on a quota of 5 is rejected while 1-5 succeed) and
its per-minute rate limit (`ApiClient.RequestsPerMinute`, `IApiKeyRateLimiter` — confirmed live
with `RequestsPerMinute: 2`).

## Response shape for a completed scan

```json
{
  "scanId": "b3f1...",
  "originalUrl": "http://paypa1-secure-login.tk/verify",
  "status": "Completed",
  "riskScore": 84,
  "riskLevel": "Critical",
  "verdict": "Malicious",
  "confidence": 0.92,
  "riskFactors": [
    { "category": "ThreatIntelligence", "description": "Confirmed malicious by PhishTank", "scoreContribution": 40 },
    { "category": "BrandImpersonation", "description": "Typosquat of paypal.com (Levenshtein 2)", "scoreContribution": 20 },
    { "category": "DomainIntelligence", "description": "Domain registered 6 days ago", "scoreContribution": 10 },
    { "category": "RedirectAnalysis", "description": "4 redirects, 2 domain changes", "scoreContribution": 8 },
    { "category": "UrlAnalysis", "description": "Contains authentication keyword 'verify'", "scoreContribution": 6 }
  ],
  "lastCheckedAtUtc": "2026-08-11T18:14:26Z"
}
```

Per spec section 33, `verdict` is always one of `Safe | LowRisk | Suspicious | Malicious |
Unknown` — never a bare boolean "is this safe", and a `Safe` verdict is presented as "No Known
Threat Detected", not "Definitely Safe".

## Real-time scan progress (SignalR)

Hub: `/hubs/scan-progress` (JWT bearer via `?access_token=` query param, same pattern as the
other AIO_Systems services' notification hubs). Client calls `JoinScanGroup(scanId)` after
`POST /api/v1/scans` returns, then listens for a `scanStageChanged` event with payload
`{ scanId, stage, message }` (`stage` is a `ScanStatus` string,
`Queued -> UrlAnalysis -> ... -> Completed`, spec section 20). Implemented by
`SignalRScanNotifier` on the server and `ScanSignalrService` on the Angular client — the
client's scan-detail page treats this as a UX nicety layered on top of polling
`GET /api/v1/scans/{id}`, not the source of truth (a missed SignalR message doesn't leave the
page stuck).
