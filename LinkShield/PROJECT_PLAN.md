# LinkShield AI — Project Plan, Workflow & Phases

LinkShield AI is a new vertical inside the `AIO_Systems` monorepo, sitting alongside
`NovaERP/`, `HRMSManagement/`, `WorkflowBuilder/`, `FlowSphereAI/`, and `ProjectFlowAI/`. It is an
enterprise URL-reputation / phishing-detection / threat-intelligence platform, built in the
24-step dependency order from the original spec. This file is the single source of truth for
**how** LinkShield gets built, in what order, and what's actually done vs. still pending —
update it whenever the plan changes, do not let it go stale.

## How LinkShield fits the existing ecosystem

| Concern | Decision |
|---|---|
| Clean Architecture layering | `LinkShield.Domain`, `LinkShield.Application`, `LinkShield.Infrastructure`, `LinkShield.API`, `LinkShield.Worker` — mirrors the `NovaERP.*` / `HRMS.*` project shapes exactly (Repository/Service, not CQRS/MediatR, for consistency with every other AIO_Systems vertical). |
| Database | Own database (`LinkShieldDB`), PostgreSQL, EF Core Code First, migrations checked in. |
| Frontend | Angular 19 client at `LinkShield/client/`, standalone components, Angular Material + PrimeNG (Aura, dark preset) — same stack as `NovaERP/client`, own port `4208`. |
| ML/security service | `LinkShield/ml-service/` — FastAPI, Python 3.12+, a genuinely separate deployable, not a .NET project. This is new to the platform; no sibling vertical has one. |
| Solution file | `/LinkShield/` folder added to `AIO_Systems.slnx` with all six backend `.csproj`s. |
| Docs | Root `README.md` has a LinkShield row, a run step, and a client-install step, same as every other vertical. |
| API Gateway | Wired — `linkshield-route`/`linkshield-hubs-route`/`linkshield-cluster` in `ApiGateway/appsettings.json`, client routed through it in production, verified live. See the phase writeup below. |
| Docker | Written and build/boot-verified — see the phase writeup below. |

## Build order (spec section 34, 24 steps)

Status column: ✅ done · 🚧 partial · ⬜ not started

| # | Step | Status |
|---|---|---|
| 1 | Architecture | ✅ `docs/architecture.md` |
| 2 | PostgreSQL schema | ✅ Domain entities + EF configurations + `InitialCreate` migration |
| 3 | .NET solution | ✅ `LinkShield.Domain/.Application/.Infrastructure/.API/.Worker/.Tests`, registered in `AIO_Systems.slnx` |
| 4 | Domain models | ✅ 20 entities under `LinkShield.Domain/Entities` |
| 5 | EF Core configurations | ✅ `LinkShield.Infrastructure/Persistence/Configurations` |
| 6 | Migrations | ✅ `20260811181426_InitialCreate` |
| 7 | Authentication | ✅ `AuthController` (register/login/refresh/logout/forgot-password/reset-password/verify-email/change-password), `AuthService`, PBKDF2 password hashing, JWT + rotating refresh tokens, account lockout, `GlobalExceptionMiddleware`; 9 xUnit tests + live-endpoint verification pass |
| 8 | URL analysis | ✅ `UrlAnalyzer` (all spec section 5 signals) + `ScansController`/`UrlController` + `ScanService` persistence; 14 xUnit tests + live-endpoint verification pass |
| 9 | DNS analysis | ✅ `DnsAnalyzer` (DnsClient) — A/AAAA/MX/NS/CNAME/TXT, suspicious-nameserver-provider list |
| 10 | Domain/RDAP analysis | ✅ `DomainAnalyzer` — RDAP via SSRF-safe client, registrar/dates/nameservers/domain age |
| 11 | SSL analysis | ✅ `SslAnalyzer` — real TLS handshake, cert/SAN/chain-error inspection via SSRF-safe connector |
| 12 | Redirect analysis | ✅ `RedirectAnalyzer` — manual hop-by-hop following, each hop SSRF-validated |
| 13 | Threat intelligence abstraction | ✅ `IThreatIntelligenceProvider` + 4 real adapters (URLhaus, PhishTank, Google Safe Browsing, VirusTotal) + `ThreatIntelligenceService` (Redis-cached, DB-persisted) |
| 14 | Risk engine | ✅ `RiskEngine` — weighted scoring from `SystemSettings`, full `RiskFactor` explanation trail, brand detection via Levenshtein/Jaro-Winkler |
| 15 | Python ML service | 🚧 FastAPI app, schemas, `/api/v1/predict`, training script all scaffolded and testable; .NET side calls it via `MlPredictionClient`; no model trained yet (no dataset) |
| 16 | Angular dashboard | ✅ Stat tiles + scans-over-time/risk-distribution/top-brands charts (Chart.js), reads new `/api/v1/dashboard/summary`+`/trends` endpoints |
| 17 | Scan page | ✅ Submit form + full detail page (10-tab layout: Overview/URL/Domain/DNS/SSL/Redirects/Threat Intel/Brand/ML/Risk Factors), polls + SignalR live stage updates |
| 18 | Admin panel | ✅ Brands/risk-rules/API-clients+keys/audit-logs management (`BrandsController`/`RiskRulesController`/`ApiClientsController`/`AuditLogsController`, RBAC-gated `SuperAdmin,Admin`) + full Angular admin UI (4 tabs) |
| 19 | SignalR | ✅ `ScanProgressHub` + `SignalRScanNotifier` broadcasts every stage transition during real pipeline runs |
| 20 | Background workers | ✅ `LinkShield.API`'s in-process `ScanProcessingHostedService` (queue-driven pipeline execution) + `LinkShield.Worker`'s `MaintenanceWorker` (stuck-scan cleanup, soft-delete purge) |
| 21 | Redis | ✅ Threat-intelligence provider results cached with per-provider TTL (`IDistributedCache`) |
| 22 | Docker | ✅ 4 Dockerfiles + `docker-compose.yml` services, built and booted successfully — real scan pipeline ran inside the containers, see `docs/deployment.md` |
| 23 | Tests | ✅ `LinkShield.Tests` has 65 passing tests (Auth 9, UrlAnalyzer 14, SsrfGuard 9, StringSimilarity 5, BrandDetectionService 4, RiskEngine 4, Admin services 5, ApiClient/key services 5, plus supporting); `ml-service/tests` has real unit tests for `ModelService` |
| 24 | Documentation | ✅ `docs/architecture.md`, `database.md`, `security.md`, `api.md`, `ml.md`, `deployment.md` |

Also done, ahead of the numbered 24 steps: **enterprise API + API-key management** (spec section
30) — `POST /api/v1/external/url/check` authenticated via `X-API-Key` (a separate ASP.NET Core
auth scheme from the JWT one), `ApiClientsController` for admin-issued keys with daily-quota
enforcement, verified live including the quota cutoff itself.

**What's explicitly not built yet:** only the browser extension (spec section 29, explicitly
Phase 2 in the original spec) and MFA/real-SMTP (spec section 21, deliberately deferred, see the
Authentication phase writeup). Every backend analysis stage, the threat-intelligence
abstraction, the risk engine, background processing, the enterprise API (with both daily-quota
and per-minute rate limiting), auth brute-force throttling, the full Angular UI (dashboard, scan
page, admin panel), API Gateway integration, and Docker are done and verified live — including
inside real containers for Docker — against real external services (real DNS, real TLS certs,
real RDAP, real threat-intel APIs), not just unit-tested in isolation.

## API Gateway integration — done

`ApiGateway/ApiGateway/appsettings.json` gained `linkshield-route` (`/api/linkshield/{**catch-all}`
→ `linkshield-cluster`, `PathPattern: /api/{**catch-all}`), `linkshield-hubs-route` (same idea for
`/hubs/linkshield/{**catch-all}`), and a `linkshield-cluster` pointing at
`http://localhost:5008` — copied from the `novaerp-route`/`workflow-route`/etc. pattern already
established for every other vertical, including the `docker-compose.yml` override
(`ReverseProxy__Clusters__linkshield-cluster__Destinations__primary__Address:
http://linkshield-api:8080` on the `apigateway` service, plus `linkshield-api` added to its
`depends_on`).

The Angular client's environment files were updated to route through the Gateway in production
(`environment.ts`: `apiBaseUrl: http://localhost:5000/api/linkshield`) while staying direct in
dev (`environment.development.ts`: `http://localhost:5008/api`). This required a real code change,
not just config: since the Gateway strips `/api/linkshield` and re-adds `/api`, every client
service (`ScanApiService`, `DashboardApiService`, `AuthApiService`, `AdminApiService`) had its
literal path strings changed from `/api/v1/...` to `/v1/...`, with the `/api` prefix now living
in `apiBaseUrl` itself — matching exactly how `NovaERP/client`'s `apiUrl` already works (its
services append `/auth/login`, not `/api/auth/login`). Get this backwards and requests silently
404 with a doubled `/api/api/...` path once routed through the Gateway — worth remembering if a
new client call site is ever added without following the convention.

Verified live (native `dotnet run`, not Docker — Docker wasn't available, see below):
`LinkShield.API` on `:5008` + `ApiGateway` on `:5000`, then confirmed
`GET /api/linkshield/v1/dashboard/summary`, `POST /api/linkshield/v1/scans` (real scan pipeline
ran), `POST /api/linkshield/v1/auth/login` (real JWT with correct roles), and a SignalR negotiate
against `/hubs/linkshield/scan-progress/negotiate` all round-tripped correctly to the real
backend through the Gateway. `dotnet build` (ApiGateway) and `ng build`/`ng test` (client) both
pass after the change.

## Docker — built and boot-verified

Four Dockerfiles (`LinkShield.API/Dockerfile`, `LinkShield.Worker/Dockerfile`,
`ml-service/Dockerfile`, `client/Dockerfile` + `client/nginx.conf`) and matching services added
to the root `AIO_Systems/docker-compose.yml` (`linkshield-api`, `linkshield-worker`,
`linkshield-ml`, `linkshield-client`), following the exact multi-stage pattern already proven by
`NovaERP.API/Dockerfile` (.NET) and `FlowSphereAI/client/Dockerfile` (Node-based client). Also
added `.dockerignore` at the `LinkShield/` root (excludes `bin`/`obj`/`client/node_modules`/
`client/dist` from every .NET build's context — otherwise each `.NET` Docker build would ship
the whole Angular `node_modules` tree in its build context) and one inside `client/`.

This was initially left unverified because Docker Desktop's daemon wasn't running (confirmed via
`docker info` and a real build attempt, both failing with "cannot connect to the Docker API").
Once asked to close out known gaps, Docker Desktop was started (`Docker Desktop.exe`, waited for
the daemon to come up) and a full `docker compose build` + `up` was run for real:

- All four images built successfully (`linkshield-api`, `linkshield-worker`, `linkshield-ml`,
  `linkshield-client`).
- All four containers started and stayed up (`linkshield-worker` confirmed 0 restarts, not
  crash-looping).
- `linkshield-api`'s `/` and `linkshield-ml`'s `/health` responded correctly; `linkshield-client`'s
  nginx served the SPA (`200` on `/`).
- A real scan submitted against the containerized API ran the **entire pipeline for real** inside
  the containers: a genuine RDAP call to `rdap.org`, a genuine HTTP fetch of `example.com`, and a
  container-to-container call from `linkshield-api` to `http://linkshield-ml:8001/api/v1/predict`
  (confirming the `MlService__BaseUrl` compose override actually takes effect) — ml-service
  correctly returned `503` (no trained model) and the pipeline skipped that signal gracefully, same
  behavior as the non-Docker environment. The scan reached `Completed`.

No bugs were found in the Dockerfiles/compose config themselves — everything worked on the first
real build. Containers were stopped afterward (`docker compose stop`, `postgres`/`redis` left
running since other AIO_Systems verticals share them).

## Authentication phase — done

`LinkShield.Application`: `IAuthService`, `IPasswordHasher`, `IJwtTokenGenerator`, `IEmailSender`
interfaces + DTOs (`DTOs/Auth/AuthDtos.cs`) + FluentValidation validators
(`Validators/Auth/AuthValidators.cs`, shared `StrongPassword()` rule).

`LinkShield.Infrastructure/Authentication`: PBKDF2-SHA256 `PasswordHasher` (100k iterations,
same scheme as `NovaERP`), `JwtTokenGenerator` (HMAC-SHA256, roles-as-claims), `AuthService`
implementing register/login/refresh/logout/change-password/forgot-password/reset-password/
verify-email directly against `LinkShieldDbContext` (Repository+Service, no extra repository
abstraction — matches the rest of the platform). `LinkShield.Infrastructure/Notifications/
LoggingEmailSender` is a deliberate dev-only stand-in that logs verification/reset tokens instead
of sending real email — swap it for a real provider before any non-local deployment.

`LinkShield.API`: `AuthController` (8 endpoints under `/api/v1/auth`), `GlobalExceptionMiddleware`
mapping `InvalidOperationException`→409, `UnauthorizedAccessException`→401,
`KeyNotFoundException`→404, FluentValidation's `ValidationException`→400. JWT bearer auth was
already wired in the initial scaffold; `[Authorize]` now actually gates `change-password`.

Verified: 9 passing xUnit tests (`LinkShield.Tests/Authentication/AuthServiceTests.cs` — register,
duplicate-email conflict, login success/failure, 5-strikes lockout, refresh rotation +
old-token revocation, forgot-password non-enumeration, reset-password, email verification) plus
a live run against real PostgreSQL exercising register → duplicate (409) → login → wrong
password (401) → weak password (400) → refresh → reused old refresh token (401) →
change-password unauthenticated (401) → authenticated (204) → login with new password (200).

Not done in this phase (deliberately out of scope, revisit later): MFA (schema fields exist and
are unused) and real SMTP/transactional email. (Login/register rate limiting was added in a
later pass — see "Rate limiting gaps closed" below.)

## URL Analysis phase — done

`LinkShield.Application`: `IUrlAnalyzer` (pure, stateless, no network/DB — safe to call on any
string) implemented by `Services/UrlAnalyzer.cs`, covering every spec-section-5 signal:
length/dot/hyphen/digit/special-char counts, IP-literal host detection, punycode detection
(label-level `xn--` check), homoglyph heuristic (ASCII+non-ASCII letter mixing, decoding
punycode first via `IdnMapping`), known URL-shortener host list, suspicious-TLD list,
suspicious-keyword list (login/verify/account/password/etc.), suspicious query-parameter names
(redirect/next/return/etc.), suspicious file extensions, and single- vs. double-percent-encoding
detection. Also computes a **stage-local** `UrlAnalysisScore` (0-100) — explicitly documented as
provisional, not the final risk score (that's the risk engine, still pending). Subdomain-count
and registered-domain use a small built-in multi-part-suffix list (`co.uk`, `com.au`, etc.)
rather than a full public-suffix-list package — good enough for common cases, called out as a
known limitation to revisit during the Domain/RDAP phase.

`IScanService`/`ScanService` (`LinkShield.Infrastructure/Services/ScanService.cs`) creates a
`UrlScan` + `UrlAnalysis` row, records a `ScanEvent`, and leaves `Status = ScanStatus.UrlAnalysis`
(the last stage that actually ran) rather than `Completed` — `RiskScore`/`RiskLevel`/`Verdict`
stay at their honest "not yet determined" defaults until the risk engine exists.

`LinkShield.API`: `UrlController` (`POST /api/v1/url/analyze` — synchronous, no persistence) and
`ScansController` (`POST /api/v1/scans`, `GET /api/v1/scans/{id}`, `GET /api/v1/scans`
paginated, `DELETE /api/v1/scans/{id}` soft-delete). Scan submission is `[AllowAnonymous]` but
still attaches `UserId` when a valid JWT is present — a public scanning tool doesn't have to
require login, matching the VirusTotal-style UX spec section 19 implies.

Verified: 14 passing xUnit tests (`LinkShield.Tests/UrlAnalysis/UrlAnalyzerTests.cs` — safe URL,
IP host, punycode, homoglyph, shortener, suspicious TLD, auth keywords, redirect parameter,
suspicious file extension, double-encoding, subdomain counting incl. multi-part TLD,
normalization, relative score ordering) plus a live run against real PostgreSQL: `/url/analyze`
on a safe URL (score 0) and a suspicious one (IP host + `.exe` + keywords, score 60), a 400 on a
malformed URL, `POST /scans` on a punycode+homoglyph+redirect-param URL persisting correctly,
followed by get-by-id, paginated list, soft-delete, and 404-after-delete.

Not done in this phase (deliberately out of scope, revisit later): `ScanProgressHub`
broadcasting (nothing async to broadcast yet — this stage runs synchronously in milliseconds),
and `LinkShield.Worker` involvement (same reason).

## Full analysis pipeline + risk engine + background processing — done

This was one large phase covering spec steps 9-14 and 19-21 together, since they're only
independently useful once wired into one working pipeline. Everything below was verified live
against real external services (real DNS, real TLS handshakes, real RDAP lookups, real
threat-intel APIs), not just unit tests.

**SSRF protection (`LinkShield.Infrastructure/Security`)** — built first since every analysis
stage after URL analysis needs it: `ISsrfGuard` resolves a host and rejects it if any candidate
address is private/loopback/link-local/multicast/metadata, or the hostname itself looks internal.
`ISsrfSafeConnector` re-validates at actual-connect-time (closing the DNS-rebinding TOCTOU gap)
and is wired into a `SocketsHttpHandler.ConnectCallback` shared by the RDAP and redirect-analysis
HttpClients, and into `SslAnalyzer`'s raw socket connection. See `docs/security.md`.

**Analyzers (`LinkShield.Infrastructure/Analysis`)**:
- `DnsAnalyzer` — DnsClient-based A/AAAA/MX/NS/CNAME/TXT resolution, flags known
  abused-free-DNS-provider nameservers.
- `DomainAnalyzer` — queries `rdap.org` (which redirects to the authoritative registry RDAP
  server per IANA bootstrap); parses registrar/registration/expiration/nameservers; never
  throws, reports `RdapLookupSucceeded=false` + `RdapError` on failure (confirmed live: rdap.org
  currently 403s a plain HttpClient user agent for some domains — a real, honestly-reported
  limitation, not a bug).
- `SslAnalyzer` — opens a real TLS connection and inspects the actual presented certificate
  (issuer/subject/SAN via .NET 9's `X509SubjectAlternativeNameExtension`/expiry/self-signed/
  domain-match/TLS version) — HTTPS is never treated as proof of legitimacy, only reported.
- `RedirectAnalyzer` — follows redirects one hop at a time (`AllowAutoRedirect=false`), SSRF-
  validating and recording each hop before following it, bounded by
  `SsrfProtection:MaxRedirects`.

**Threat intelligence (`LinkShield.Infrastructure/ThreatIntelligence`)** — `IThreatIntelligenceProvider`
implemented by `UrlhausProvider`, `PhishTankProvider`, `GoogleSafeBrowsingProvider`,
`VirusTotalProvider`. Confirmed live: URLhaus and PhishTank now both require an API key/Auth-Key
(abuse.ch and PhishTank changed their policies since the original spec assumed keyless access);
all four now report `Unavailable` (not `Error`) when unconfigured, so the gap in coverage is
visible rather than masked. `ThreatIntelligenceService` queries every enabled+adapter-backed
provider in parallel, caches each non-error result in Redis with that provider's configured TTL,
and always persists one `ThreatIntelligenceResult` row per provider per scan regardless of
cache hit/miss.

**Brand detection (`LinkShield.Infrastructure/Risk/BrandDetectionService`)** — compares every
hyphen/dot-separated label of the scanned domain (not just the whole registrable domain) against
each `BrandProfile` name and official-domain label using `StringSimilarity`
(`LinkShield.Application/Common`, hand-written Levenshtein + Jaro-Winkler). This per-label
comparison was added after live testing showed the naive whole-domain comparison completely
missed `paypa1-secure-login.tk` (a real typosquat pattern) — comparing "paypa1-secure-login.tk"
against "paypal.com" as whole strings gives a large distance/low similarity, but comparing the
label "paypa1" against "paypal" catches it immediately (Levenshtein 1). Match types:
`Homoglyph` (non-ASCII label, high similarity), `BrandNameInDomain` (exact brand token embedded
in a non-official domain), `Typosquat` (small edit distance), `KeywordCombo` (brand name
substring + auth keyword elsewhere in the URL).

**Risk engine (`LinkShield.Infrastructure/Risk/RiskEngine`)** — combines every stage's score
into one weighted 0-100 `RiskScore`, reading category weights and Low/Moderate/High thresholds
from `SystemSettings` (admin-tunable without a redeploy). Produces a full `RiskFactor`
explanation trail matching the spec's example format exactly (verified live — see below). Only
a confirmed threat-intelligence hit ever produces `ScanVerdict.Malicious`; scoring signals alone
top out at `Suspicious`, and `Confidence` reflects how many of the 8 signal categories actually
returned data (an ml-service that's unreachable, or all TI providers being `Unavailable`, lowers
confidence and yields `Unknown` rather than a falsely-confident `Safe`/`LowRisk`).

**ML integration (`LinkShield.Infrastructure/Risk/MlPredictionClient`)** — calls ml-service's
`/api/v1/predict`; returns `null` (not a fabricated prediction) on `503`/timeout/any failure, so
the risk engine simply omits that signal.

**Background processing** — `IScanQueue` (in-process `Channel<Guid>`) + `ScanProcessingHostedService`
(`LinkShield.API`) drain it and run `IScanPipelineRunner` (`LinkShield.Infrastructure/
BackgroundProcessing`) in its own DI scope per scan, so the real network calls in every stage
never block the HTTP request thread. `IScanNotifier`/`SignalRScanNotifier` broadcast every stage
transition to `ScanProgressHub`. `LinkShield.Worker`'s `MaintenanceWorker` runs independently
(its own process, own DB connection) to catch what the in-memory queue can't: scans left
mid-pipeline by an API restart get marked `Failed` after a threshold, and old soft-deleted scans
get purged.

**Verified live** (real Postgres, real external services, no mocks): a scan of
`https://www.wikipedia.org/` completed end-to-end through all 9 stages — real A/AAAA/CNAME
records, a real Let's Encrypt certificate with 38 SANs inspected, RDAP 403 handled gracefully,
all 4 TI providers correctly reporting `Unavailable`/`Error`, confidence 0.88 (7/8 signals, ML
unreachable), verdict `Unknown`. A scan of `http://paypa1-secure-login.tk/account/verify`
correctly flagged suspicious TLD + auth keywords (stage-1 score 35) and, after the brand-detection
fix above, a `Typosquat` match against PayPal (Levenshtein 1, Jaro-Winkler 0.93), landing at
risk score 16 with a complete, human-readable `RiskFactor` trail.

Also added: `LinkShield.Tests/Security/SsrfGuardTests` (IPv4/IPv6 private-range blocking),
`LinkShield.Tests/BrandDetection/StringSimilarityTests` + `BrandDetectionServiceTests`,
`LinkShield.Tests/Risk/RiskEngineTests` (Safe/Malicious/never-falsely-Malicious/Unknown-on-
missing-signals) — 55 tests total, all passing.

## Dashboard + Scan page (Angular) — done

Two small backend endpoints were added first since the dashboard had nothing to read from:
`GET /api/v1/dashboard/summary` (total/safe/suspicious/malicious/critical counts, threats
detected, average risk score, TI match count) and `GET /api/v1/dashboard/trends?days=` (scans
per day, risk-level distribution, threat categories, top suspicious domains, top targeted
brands) — both in `IDashboardService`/`DashboardService`. "Top suspicious domains" is keyed off
`RiskScore > 0` rather than `Verdict`, deliberately — a low-confidence scan (e.g. ml-service
unreachable) can carry real signal while still reporting `Verdict=Unknown`, and that evidence is
still worth surfacing on a dashboard even though the scan-detail page correctly won't claim a
verdict it isn't confident about.

`LinkShield/client/src/app/core`: typed models mirroring every backend DTO
(`models/scan.models.ts`, `dashboard.models.ts`, `auth.models.ts`), `ScanApiService`,
`DashboardApiService`, `AuthApiService` (stores the JWT via `TokenStorageService`, exposes
`currentUser` as a signal), and `ScanSignalrService` (per-component hub connection — a
scan-detail page only ever needs one scan's group at a time, so it isn't a singleton).

`features/dashboard`: stat tiles + three Chart.js charts (stacked scans-over-time by verdict
using the existing status-color tokens, risk-level distribution, top targeted brands as a
single-hue magnitude bar) — deliberately no categorical rainbow palette; severity charts reuse
the reserved status colors, magnitude-only charts use one hue, per the platform's dark SOC theme
already established in `styles.scss`.

`features/scan`: `ScanSubmitComponent` (URL input → `POST /api/v1/scans` → navigate to the new
scan) and `ScanDetailComponent` — a 10-tab layout (Overview, URL, Domain, DNS, SSL/TLS,
Redirects, Threat Intelligence, Brand Detection, ML Analysis, Risk Factors) matching spec section
19's exact tab list, minus a Timeline tab (backend doesn't expose `ScanEvent` rows via the API
yet — noted as a gap, not silently dropped). While a scan is non-terminal, the page polls
`GET /api/v1/scans/{id}` every 2s (`rxjs` `timer`+`switchMap`+`takeWhile`) and also joins the
scan's SignalR group for a live "stage: message" line — polling is the source of truth, SignalR
is a UX nicety layered on top since it isn't guaranteed to arrive before a poll does anyway.

`features/auth`: login/register forms wired to the real `AuthController` endpoints built
earlier. Scanning itself stays `[AllowAnonymous]` end-to-end — login only matters for attaching
`userId` to scans and (later) admin-panel access.

Routing (`app.routes.ts`): `/` redirects to `/dashboard`, `/scan` and `/scan/:id`,
`/auth/login`/`/auth/register`, and the original health-check page moved to `/health` (kept as a
diagnostics page rather than deleted). `ShellComponent` (`shared/components/shell`) replaced the
bare `router-outlet` with a real nav header (Dashboard/New Scan links + login state).

Verified: `ng build` and `ng test` both pass. No browser-automation tool (Playwright/
chromium-cli) was available in this environment to capture a screenshot, so this was verified at
the server level only — the dev server (already running on `:4208`) serves the current bundle
(confirmed the new `AuthApiService` symbol is present in the served `main.js`), `/dashboard`
resolves via the Angular router's SPA fallback, and `LinkShield.API`'s
`/api/v1/dashboard/summary`/`/trends` return real data end-to-end. A human should still open the
app in an actual browser to confirm layout/visual correctness before considering this UI done.

## Admin panel — done

Backend: `IBrandManagementService`/`BrandManagementService` (list + create, rejects a duplicate
`OfficialDomain` with 409), `IRiskRuleManagementService`/`RiskRuleManagementService` (list +
create + update — update lets an admin retune `Weight`/`ScoreContribution`/`IsEnabled` without
a redeploy, per spec section 12), `IAuditLogService`/`AuditLogService` (paginated). All three
controllers (`BrandsController`, `RiskRulesController`, `AuditLogsController`) are
`[Authorize(Roles = "SuperAdmin,Admin")]` — confirmed live: 401 unauthenticated, 403 for a
regular `User`-role account, 200 for the seeded admin.

Every create/update call writes a real `AuditLog` row (`BrandProfileChanged`/`RiskRuleChanged`),
and `AuthService`/`ScanService` were retrofitted to write `Register`/`Login`/`Logout`/
`PasswordReset`/`ScanCreated`/`ScanDeleted` audit entries too — the audit log had existed as an
empty table until this phase; it needed real writers, not just a reader.

`SeedData` now seeds one demo `SuperAdmin` account (`admin@linkshield.local` / `Admin@123`),
matching the "seed a demo login" convention every other AIO_Systems vertical follows. Caught and
fixed live: the seed guard was originally `if (!await db.Users.AnyAsync())`, which skipped
seeding entirely once any user existed in the real (persistent, not in-memory) dev database from
earlier manual testing — changed to check for that specific email instead.

`LinkShield/client/src/app/features/admin`: originally a 3-tab page (Brands, Risk Rules, Audit
Logs), a 4th tab (API Clients) was added in the API-key-management phase below.
`AdminApiService` added to `core/services`. `AuthApiService` was extended with `hasAnyRole()` and
a `decodeStoredToken()`
step so the admin nav link and the new `adminGuard` route guard survive a page refresh — reading
the JWT's claims client-side to rehydrate UI state only; the server still authorizes every
request independently, this grants no access on its own.

Verified live end-to-end: admin login → create brand → duplicate rejected (409) → create risk
rule → toggle it off via update → audit log shows every action in order, plus the earlier
register/login events. `ng build`/`ng test` pass; the running dev server resolves `/admin` via
the SPA fallback.

## API-key management + enterprise API — done

Spec section 30, done ahead of Docker/API-Gateway since it's a real feature rather than
infrastructure wiring. `ApiClient`/`ApiKey` entities already existed from the initial schema
scaffold; this phase gave them a service, endpoints, and an auth path.

`ApiKeyHasher` (`LinkShield.Infrastructure/ApiKeyAuth`) generates `ls_live_`-prefixed keys and
hashes them with plain SHA-256 — deliberately not PBKDF2: API keys are already high-entropy
random tokens, not human passwords, so a slow hash buys nothing and would only slow down the
request-time lookup (see the doc comment for the full reasoning; `PasswordHasher` stays PBKDF2
for actual user passwords). `ApiClientService` is admin-only CRUD (create client, issue key —
the raw key is returned exactly once, only its hash is ever stored — revoke key), every
create/revoke writes an `AuditLog` row. `ApiKeyValidator` is the separate request-time path:
hash lookup, active/expiry/client-active checks, and daily-quota enforcement by counting that
client's `UrlScans` since midnight UTC — a DB read-then-compare rather than an atomic Redis
counter, documented as not airtight under heavy concurrent bursts from the same key but
sufficient at current traffic; worth revisiting with Redis `INCR`/`EXPIRE` if that ever matters.

`ApiKeyAuthenticationHandler` (`LinkShield.API/Authentication` — needs real ASP.NET Core hosting
types, so it lives in the Web SDK project, not Infrastructure) registers a second authentication
scheme (`"ApiKey"`, reading `X-API-Key`) alongside the existing JWT bearer scheme; controllers
pick per-endpoint via `[Authorize(AuthenticationSchemes = ...)]`. `ExternalController`'s
`POST /api/v1/external/url/check` uses it and calls the same `IScanService.SubmitScanAsync` the
regular scan flow uses (now taking an optional `apiClientId` alongside `userId`, so a scan can be
attributed to either an interactive user or an API client, never both).

Verified live: created an API client with a deliberately low quota (5/day), issued a key,
called the external endpoint without a key (401), with the key (200, real scan pipeline runs),
and confirmed the exact 6th request in one day is rejected while the first 5 succeed — the quota
check is not off-by-one in either direction. `ApiClientServiceTests`/`ApiKeyValidator` tests
(5 total) cover key generation, hash-not-raw-key storage, revocation, and unknown-key rejection.

Angular: `AdminComponent` gained a 4th "API Clients" tab — create-client form, per-client key
table (prefix/label/last-used/usage-count/active), "Issue key" (shows the raw key once in a
dismissible banner, matching the "shown only once" backend contract) and "Revoke". `ng build`
passes with the new tab; `AdminApiService` extended with the four new calls.

## Rate limiting gaps closed — done

Two previously-documented-but-unfixed gaps, both closed as a follow-up pass once Docker was
verified:

**Per-minute API-key rate limiting.** `ApiClient.RequestsPerMinute` existed in the schema and was
returned by the admin UI, but nothing checked it — only the daily quota was enforced.
`IApiKeyRateLimiter`/`InMemoryApiKeyRateLimiter` (`LinkShield.Infrastructure/ApiKeyAuth`) adds a
singleton fixed-window counter keyed by `(apiClientId, currentUtcMinute)`, wired into
`ApiKeyValidator` alongside the existing daily-quota check. Deliberately in-process, not Redis —
documented as a single-instance limitation (counts would be per-replica if `LinkShield.API` ever
scales horizontally), matching the same tradeoff already accepted for the daily quota. Verified
live: a client created with `RequestsPerMinute: 2` let 2 requests through then `401`'d the 3rd
with "Per-minute rate limit exceeded." New tests: `InMemoryApiKeyRateLimiterTests` (allows up to
the limit then rejects; tracks different clients independently) and
`ApiKeyValidator_EnforcesPerMinuteRateLimit`.

**Login/register/forgot-password brute-force throttling.** Never implemented; `docs/security.md`
explicitly flagged it as a known gap (`AspNetCoreRateLimit` had been scoped out of the initial
csproj as unused). Rather than pulling that package back in, used .NET's built-in
`Microsoft.AspNetCore.RateLimiting` (no new package — it's part of the shared framework since
.NET 7): a fixed-window limiter policy `"auth"`, partitioned per client IP, 10 requests/minute,
`QueueLimit = 0` (immediate `429` rather than making callers wait), applied via
`[EnableRateLimiting("auth")]` on `AuthController.Register`/`Login`/`ForgotPassword` only
(`Refresh`/`ResetPassword`/`VerifyEmail`/`Logout` all require a token the attacker doesn't have,
so they aren't brute-forceable the same way). Verified live: 10 rapid login attempts from the
same IP each got a normal `401` (wrong credentials), the 11th got `429`.

Both limiters share the same documented tradeoff: correct for a single `LinkShield.API`
instance, not distributed. Revisit with Redis `INCR`/`EXPIRE` only if horizontal scaling actually
happens — don't preemptively add that complexity.

## Design notes / deviations from the literal spec worth remembering

- **SSRF protection is a cross-cutting concern, not a per-feature one.** Implemented once in
  `LinkShield.Infrastructure/Security` and shared by every stage that opens a real connection —
  see `docs/security.md`. Don't let any future analysis component grow its own
  `HttpClient`/socket call that skips it.
- **`ml-service` fails loudly instead of guessing.** `ModelService.predict()` raises
  `ModelNotTrainedError` (→ HTTP 503) until a real model artifact exists, and
  `MlPredictionClient` on the .NET side returns `null` rather than fabricating a probability.
- **Risk-rule weights are data (`SystemSettings`), not constants in code** — `RiskEngine` reads
  them at scoring time so an admin can retune weights without a redeploy. The individual
  `RiskRule.ConditionExpression` rows are seeded reference data describing intended rules but are
  not yet evaluated by an expression engine; scoring is code-driven per category today. Wiring a
  real rule evaluator through those rows is a documented follow-up, not silently dropped scope.
- **Public threat-intel APIs changed their access policy since the spec was written** — URLhaus
  and PhishTank now require keys/Auth-Keys that the original "free public API" assumption didn't
  account for. Discovered by testing against the real endpoints, not assumed from documentation.
- **The in-process scan queue is a deliberate simplification, not an oversight.** The spec asks
  for ".NET workers" (plural, implying separate processes) for scan processing; this
  implementation runs the pipeline inside `LinkShield.API` via a hosted service + channel so it
  doesn't block requests, while `LinkShield.Worker` handles genuinely separate, schedule-driven
  cleanup. Revisit only if scan volume ever requires horizontal scaling of pipeline processing
  independent from the API.
