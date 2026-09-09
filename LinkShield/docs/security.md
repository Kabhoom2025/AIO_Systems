# LinkShield AI — Security

## SSRF protection (spec section 23) — the platform's highest-risk surface

LinkShield accepts arbitrary user-submitted URLs and then makes outbound network requests
against them (DNS, HTTP fetch, redirect follows, cert/RDAP lookups). Every one of those calls
must pass the same validation gate before it goes out:

1. Parse the URL; reject anything that doesn't parse.
2. Validate scheme — only `http`/`https` (see `SsrfProtection:AllowedSchemes` in
   `appsettings.json`).
3. Resolve the hostname to its IP address(es).
4. Reject if any resolved IP is private (RFC 1918), loopback (`127.0.0.0/8`, `::1`),
   link-local (`169.254.0.0/16`, `fe80::/10`), multicast, or a known cloud metadata endpoint
   (`169.254.169.254`, `metadata.google.internal`, etc.).
5. Reject internal-looking DNS names (`.local`, `.internal`, single-label hostnames).
6. Enforce `SsrfProtection:RequestTimeoutMs`, `MaxRedirects`, and `MaxResponseBytes`.
7. **Re-run steps 1-6 on every redirect hop**, not just the original URL — a safe URL that
   redirects to `http://169.254.169.254/` must be caught.

This lives behind a single shared abstraction in `LinkShield.Infrastructure` so no analysis
component (URL fetch, redirect follower, cert fetch) can bypass it by constructing its own
`HttpClient`.

## Authentication & authorization — implemented

- JWT access tokens (HMAC-SHA256, `JwtTokenGenerator`) + rotating refresh tokens (`RefreshTokens`
  table — every refresh revokes the old token and chains to the new one via `ReplacedByToken`;
  reusing a revoked/expired token is rejected with 401, not silently accepted).
- Password hashing is PBKDF2-SHA256, 100k iterations, random salt per user
  (`LinkShield.Infrastructure/Authentication/PasswordHasher.cs`) — never plaintext, never a fast
  hash like bare SHA256/MD5.
- Account lockout after 5 failed logins, 15-minute lockout window
  (`User.FailedLoginAttempts`/`LockedUntilUtc`, enforced in `AuthService.LoginAsync`).
- Email verification and password-reset tokens are single-use with expiry
  (`EmailVerificationTokenExpiresAtUtc` 24h, `PasswordResetTokenExpiresAtUtc` 1h) and are cleared
  once consumed.
- `ForgotPasswordAsync` always returns successfully regardless of whether the email exists —
  never reveals account existence via response shape or timing-observable branching.
- Every registered user is assigned the `User` role by default; `[Authorize]` on
  `change-password` proves role/claims-based auth is live end-to-end.
- Verification/reset tokens are currently delivered via `LoggingEmailSender` (logs the token,
  does not send real email) — see `docs/ml.md`-style caveat in `PROJECT_PLAN.md`. Swap this for a
  real provider before any non-local deployment; do not build a "reveal token in the API
  response" shortcut instead.
- `User.MfaEnabled`/`MfaSecret` reserve the schema for TOTP-based MFA (spec section 21:
  "MFA-ready architecture") — not implemented yet.
- RBAC roles: `SuperAdmin`, `Admin`, `SecurityAnalyst`, `User`, `ApiClient` — seeded. Every
  self-registered user gets `User`; `SuperAdmin`/`Admin` currently require direct DB assignment
  (no role-management endpoint yet) — a demo `SuperAdmin` is seeded at
  `admin@linkshield.local` / `Admin@123` for local testing. `BrandsController`,
  `RiskRulesController`, and `AuditLogsController` are `[Authorize(Roles = "SuperAdmin,Admin")]`
  — confirmed live: 401 unauthenticated, 403 for a `User`-role account, 200 for an admin.
- Enterprise API consumers authenticate via `X-API-Key` against `ApiKey.KeyHash` — SHA-256, not
  PBKDF2 (API keys are high-entropy random tokens, not human passwords, so a slow hash isn't
  needed and would only slow down every request's lookup). The raw key is shown exactly once, at
  creation (`POST /api/v1/api-clients/{id}/keys`); only a display prefix (`ls_live_ab12...`) and
  the hash are ever stored afterward. `ApiKeyAuthenticationHandler` is a second ASP.NET Core auth
  scheme alongside JWT bearer — `ExternalController` opts into it explicitly via
  `[Authorize(AuthenticationSchemes = "ApiKey")]`, everything else stays on JWT. Both the daily
  quota (`ApiClient.DailyQuota`) and the per-minute rate limit (`ApiClient.RequestsPerMinute`,
  `IApiKeyRateLimiter`/`InMemoryApiKeyRateLimiter`) are enforced per client — confirmed live with
  a client set to `RequestsPerMinute: 2`.
- Brute-force throttling on `login`/`register`/`forgot-password` — ASP.NET Core's built-in
  `Microsoft.AspNetCore.RateLimiting` (no `AspNetCoreRateLimit` package needed), a fixed-window
  limiter partitioned per client IP, 10 requests/minute, `[EnableRateLimiting("auth")]` on those
  three actions. Confirmed live: the 11th login attempt within a minute from the same IP gets
  `429`. Both this and `InMemoryApiKeyRateLimiter` are in-process, single-instance limiters — if
  `LinkShield.API` ever runs more than one replica, counts would be per-instance rather than
  global; revisit with a distributed counter (Redis `INCR`/`EXPIRE`) if that becomes real.

## What must never appear in logs or API responses

Passwords, password hashes, API keys (only `KeyPrefix`), JWT secrets, refresh token values,
MFA secrets, threat-intel provider API keys. Serilog enrichment adds correlation id / user id /
scan id for traceability — never the secret values themselves.

## Accuracy / non-claims (spec section 33)

The platform must never report "safe" as a positive claim — only "no known threat detected",
distinguished from "suspicious" and "confirmed threat-intelligence match". `ScanVerdict.Safe` is
reserved for scans with strong corroborating signals (age, reputation, no TI hits, no brand
match), not merely the absence of a hit. See `docs/api.md` for how this surfaces in responses.
