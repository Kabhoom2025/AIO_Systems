# LinkShield AI — Deployment

## Ports (dev, following the AIO_Systems port-allocation convention)

| Service | Path | Port |
|---|---|---|
| LinkShield.API | `LinkShield/LinkShield.API/` | `http://localhost:5008` |
| LinkShield Client (UI) | `LinkShield/client/` | `http://localhost:4208` |
| ml-service (FastAPI) | `LinkShield/ml-service/` | `http://localhost:8001` |
| LinkShield.Worker | `LinkShield/LinkShield.Worker/` | background service, no inbound port |

Host ports are the same whether a service runs natively (`dotnet run`/`npm start`) or as a
Docker container, matching the convention the other AIO_Systems verticals use.

## Databases

`LinkShieldDB` (PostgreSQL). EF Core migrations are checked in
(`LinkShield.Infrastructure/Persistence/Migrations`); apply with:

```powershell
cd LinkShield\LinkShield.Infrastructure
dotnet ef database update --startup-project ..\LinkShield.API\LinkShield.API.csproj
```

`SeedData.SeedAsync` runs automatically on API startup in Development — roles, the 4 threat-intel
provider registrations, starter brand profiles, starter risk rules, risk-engine settings, and a
demo `SuperAdmin` account (`admin@linkshield.local` / `Admin@123`).

## Running locally without Docker

```powershell
# 1. PostgreSQL and Redis must be reachable at the connection strings in appsettings.json
# 2. LinkShield.API
cd LinkShield\LinkShield.API
dotnet run

# 3. ml-service
cd LinkShield\ml-service
pip install -r requirements.txt
uvicorn app.main:app --port 8001 --reload

# 4. Client
cd LinkShield\client
npm start   # -> :4208

# 5. Worker
cd LinkShield\LinkShield.Worker
dotnet run
```

## Docker

Four services were added to the root `AIO_Systems/docker-compose.yml`: `linkshield-api`,
`linkshield-worker`, `linkshield-ml`, `linkshield-client`, each with its own Dockerfile
(`LinkShield.API/Dockerfile`, `LinkShield.Worker/Dockerfile`, `ml-service/Dockerfile`,
`client/Dockerfile` + `client/nginx.conf`), following the same multi-stage build pattern the
other .NET/Angular/Next.js services in this repo already use (see `NovaERP.API/Dockerfile` and
`FlowSphereAI/client/Dockerfile` for the templates these were built from).

```powershell
cd C:\dotnet_practice\AIO_Systems
docker compose up --build linkshield-api linkshield-worker linkshield-ml linkshield-client
```

`linkshield-api` and `linkshield-worker` connect to the shared `postgres`/`redis` containers
already in the compose file (`Host=postgres`, `redis:6379`) rather than `localhost`.
`linkshield-api` gets `MlService__BaseUrl=http://linkshield-ml:8001` (container-to-container
name resolution) instead of the `appsettings.json` default of `localhost:8001`.
`linkshield-worker` depends on `linkshield-api` having started first, since the Worker doesn't
call `EnsureCreated()` itself and would otherwise race the API for schema creation on a fresh
database. `linkshield-client`'s nginx just serves the static Angular build with an SPA
`try_files` fallback — it does not proxy `/api`; the browser talks to `linkshield-api` directly
on `:5008`, same as the non-Docker setup.

**Not yet verified**: Docker Desktop's daemon was not running in the environment these were
written in, so `docker compose build`/`up` has not actually been exercised for these four
services — only `docker compose config` (syntax/interpolation validation) passed, and the
Dockerfiles are pattern-matched line-for-line against the sibling services' already-working
ones. Run the command above and fix whatever breaks before considering this phase fully done;
don't assume it works untested just because it looks right.

## API Gateway routing

Like every other AIO_Systems vertical, the Angular client talks only to `ApiGateway` (`:5000`) in
production builds, which routes `/api/linkshield/*` and `/hubs/linkshield/*` to `LinkShield.API`.
`ApiGateway/ApiGateway/appsettings.json` has `linkshield-route` (strips the `/linkshield` segment
and forwards the rest as `/api/{...}`) and `linkshield-hubs-route` (same for `/hubs/{...}`), plus
a `linkshield-cluster` pointing at `http://localhost:5008` (overridden to
`http://linkshield-api:8080` in `docker-compose.yml`'s `apigateway` service).

Because the Gateway strips `/api/linkshield` and re-adds `/api`, every client service call
appends `/v1/...` (not `/api/v1/...`) to `environment.apiBaseUrl` — the `/api` prefix lives in
the base URL itself (`http://localhost:5000/api/linkshield` in production,
`http://localhost:5008/api` in the direct-dev environment), matching the convention every other
AIO_Systems Angular client already uses (compare `NovaERP/client`'s `apiUrl`). If you add a new
client-side API call, follow this pattern — don't reintroduce a literal `/api/` in the path
string or it will double up when routed through the Gateway.

Verified live (native `dotnet run`, not Docker): `LinkShield.API` on `:5008` + `ApiGateway` on
`:5000`, then `POST /api/linkshield/v1/scans`, `POST /api/linkshield/v1/auth/login`, and a
SignalR negotiate against `/hubs/linkshield/scan-progress/negotiate` all round-tripped correctly
through the Gateway to the real backend.
