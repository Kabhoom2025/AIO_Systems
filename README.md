# AIO Systems — Multi-Tenant Platform Monorepo

`AIO_Systems` is the root folder for the whole platform: the Super Admin
control plane, the API Gateway, the Super Admin UI, and every backend
microservice (Restaurant, Pharmacy, HRMS, and any future verticals). All new
backend services should be created as new sibling folders here, alongside
`ApiGateway/`, `FoodOrderingSystemManagement/`, `PharmacyManagement/`, and
`HRMSManagement/`.

Requests from any Angular client go through the API Gateway (YARP), which
routes to the correct backend service.

## Projects in this solution

Open `AIO_Systems.slnx` in Visual Studio to see every backend project together
(organized into `FoodOrderingSystemManagement`, `PharmacyManagement`, and
`HRMSManagement` solution folders).

| Project                       | Path                                             | Port(s)                |
|--------------------------------|---------------------------------------------------|-------------------------|
| AIO_Systems (Super Admin API)  | `AIO_Systems/`                                    | `http://localhost:5284` |
| ApiGateway                     | `ApiGateway/ApiGateway/`                          | `http://localhost:5000` |
| Super Admin Client (UI)        | `client/`                                         | `http://localhost:4202` |
| FoodOrder.API (Restaurant)     | `FoodOrderingSystemManagement/src/FoodOrder.API/` | `http://localhost:5275` |
| Restaurant Client (UI)         | `FoodOrderingSystemManagement/client/`            | `http://localhost:4200` |
| Restaurant Docs (Docusaurus)   | `FoodOrderingSystemManagement/docs-site/`         | `http://localhost:3000` |
| Pharmacy.API                   | `PharmacyManagement/Pharmacy.API/`                | `http://localhost:5002` |
| Pharmacy Client (UI, remote)   | `PharmacyManagement/client/`                      | `http://localhost:4201` |
| HRMS.API                       | `HRMSManagement/HRMS.API/`                        | `http://localhost:5003` |
| HRMS Client (UI)               | `HRMSManagement/client/`                          | `http://localhost:4203` |
| NovaERP.API                    | `NovaERP/NovaERP.API/`                            | `http://localhost:5004` |
| NovaERP Client (UI)            | `NovaERP/client/`                                 | `http://localhost:4204` |
| Workflow.API                   | `WorkflowBuilder/Workflow.API/`                   | `http://localhost:5005` |
| Workflow Builder Client (UI)   | `WorkflowBuilder/client/`                         | `http://localhost:4205` |
| FlowSphere.API                 | `FlowSphereAI/FlowSphere.API/`                    | `http://localhost:5006` |
| FlowSphere AI Client (UI)      | `FlowSphereAI/client/`                            | `http://localhost:4206` |
| ProjectFlowAI.API              | `ProjectFlowAI/ProjectFlowAI.API/`                | `http://localhost:5007` |
| ProjectFlow AI Client (UI)     | `ProjectFlowAI/client/`                           | `http://localhost:4207` |
| LinkShield.API                 | `LinkShield/LinkShield.API/`                      | `http://localhost:5008` |
| LinkShield Client (UI)         | `LinkShield/client/`                              | `http://localhost:4208` |
| LinkShield ml-service (FastAPI)| `LinkShield/ml-service/`                          | `http://localhost:8001` |
| Platform.Api (LineageStudio)   | `LineageStudio/src/Platform.Api/`                 | `http://localhost:5009` |
| LineageStudio Client (UI)      | `LineageStudio/client/`                           | `http://localhost:4209` |

LinkShield AI is a URL-reputation / phishing-detection / threat-intelligence platform — see
`LinkShield/PROJECT_PLAN.md` for its build status. Authentication, the full analysis pipeline
(URL/domain-RDAP/DNS/SSL/redirect/threat-intelligence/brand-detection/risk-scoring), the
enterprise API/API-key management, and the Angular UI (dashboard, scan page, admin panel) are
built and verified live, and it is now routed through `ApiGateway` (`/api/linkshield/*` and
`/hubs/linkshield/*`) like every other vertical — confirmed live (scan submission, login, and a
SignalR negotiate handshake all round-tripped through the Gateway correctly). `docker-compose.yml`
has matching services (`linkshield-api`/`-worker`/`-ml`/`-client`), but they have not been
build-tested (Docker Desktop's daemon wasn't running when they were added) — see
`LinkShield/docs/deployment.md` before trusting them.
Demo admin login: `admin@linkshield.local` / `Admin@123` (seeded automatically on first run).

The Angular clients only ever talk to the **ApiGateway** (port 5000). The
Gateway forwards requests to AIO_Systems, FoodOrder.API, Pharmacy.API, or
HRMS.API depending on the path (see `ApiGateway/ApiGateway/appsettings.json`
for the full route table).

`FlowSphereAI/` is a deliberate exception to the "every client is Angular"
rule: its client is Next.js/React/TypeScript/Tailwind/shadcn instead. It
still only ever talks to the same ApiGateway on port 5000 — see
`FlowSphereAI/README.md` (if present) or the plan doc for why this module
diverges from the rest of the platform's frontend stack.

`LineageStudio/` is a second such exception: a Visual App Builder + Runtime
Data Lineage Platform (drag/drop UI builder, PostgreSQL table designer, API
designer, React Flow mapping designer, and a live runtime lineage viewer).
Its client is React + Vite + TypeScript + Tailwind + React Flow (not
Angular), and its backend follows `src/Platform.Api` /
`Platform.Application` / `Platform.Domain` / `Platform.Infrastructure` /
`Platform.Runtime` / `Platform.Lineage` layering per its own project plan,
routed through `ApiGateway` at `/api/lineagestudio/*` and
`/hubs/lineagestudio/*`. As of 2026-09-06: solution/project scaffolding, the
`platform.*` metadata schema + migrations, application management (CRUD,
versioning, publish/unpublish), and the Data Designer (create/drop real
PostgreSQL tables and columns per application, in an `app_<applicationId>`
schema, with safe identifier quoting/validation and typed default-value
literals so no raw SQL identifier or value is ever concatenated) are built
and verified live against PostgreSQL, each with a real React screen and
passing integration tests. The UI Builder (screens with unique routes, and
all 14 spec'd component types - Text/Input/Number/Email/Select/Checkbox/
Radio/Date/Button/Table/Card/Form/Label/Container - each with position,
properties/validation/events JSON, and data binding) is also built: a
React Flow canvas with a component palette, drag-to-reposition, and a
properties panel, wired to real CRUD endpoints. The API Designer (services
and API endpoints - GET/POST/PUT/PATCH/DELETE, path normalization,
per-application method+path uniqueness, request/response JSON schemas,
service/table linkage validated against the same application) is also
built, with a React screen for both. The Mapping Designer (UI field → API
field → service field → database column, all four link references
validated against the same application, restricted to the fixed
Trim/Uppercase/Lowercase/Default/Concatenate/Split/DateConversion/
NumberConversion transformation enum - never arbitrary code - with config
required only where a transformation needs one) is also built, with a
form-based creation UI plus a read-only React Flow diagram visualizing
every mapping as a lineage-style chain. The Runtime Engine is also built:
`POST /api/runtime/{applicationId}/execute` resolves an API's mappings,
applies each field's transformation (`Platform.Runtime.Execution.
TransformationEngine`, still restricted to the fixed enum - no code
execution), and runs real parameterized INSERT/SELECT/UPDATE/DELETE
against the application's own PostgreSQL schema; a rejected constraint
(e.g. a duplicate email) comes back as a normal `Success: false` result
with a sanitized Postgres message rather than a 500, which a React "Run"
page renders as PASS/FAILED with a per-field raw→transformed breakdown.
Verified against the spec's own Customer Registration demo end-to-end,
including the deliberate duplicate-email failure case. The Lineage Engine
is also built: `Platform.Lineage.Execution.LineageTrackedRuntimeExecutionService`
decorates the runtime engine (transparently, via DI - the runtime engine
has no idea lineage exists) to record a full `platform.lineage_executions`
+ `platform.lineage_events` chain per run - ScreenStarted →
ComponentStarted → ApiStarted → TransformationStarted/Completed →
ServiceStarted → DatabaseStarted/Completed → ServiceCompleted →
ApiCompleted → ExecutionCompleted, correctly parented - with sensitive
field masking (`MetadataMasker`) and downstream nodes marked Skipped when
a validation failure stops execution before reaching the database. Read
via `GET /api/lineage/executions`, `/api/lineage/executions/{id}`, and
`/api/lineage/{id}/events`, with a basic React execution list + event
table (the live SignalR-driven React Flow visualization is Phase 10). An
application is auto-published on its first run if it has no version yet,
so lineage always has a real VersionId to snapshot against. Verified with
3 new integration tests covering the full success chain, a real
DATABASE_FAILED (duplicate email) with nothing upstream skipped, and a
transformation-level validation failure with Service/Database correctly
Skipped.

SignalR live visualization (Phase 10) is also built: `LineageHub` at
`/hubs/lineagestudio/lineage` (per-application groups), `SignalRLineageEventPublisher`
replacing the no-op publisher so every recorded event/execution update is
pushed live with no polling, and a React Flow-based `LineageFlowView`
(lane-per-node-type layout, live status colors/icons, click-a-node detail
panel with error/duration/correlation ID) driven entirely by real
`lineageEvent`/`lineageExecutionUpdate` pushes - no client-side timers or
fake animation. Verified with 2 dedicated SignalR integration tests using
a real `HubConnection` against the test server: a joined client receives
every event including the terminal `ExecutionCompleted`, and a client that
never joins the group receives nothing.

Execution History (Phase 11) is also built: `LineageExecutionDto` now
carries the application's name, and a React `ExecutionHistoryPage` lists
every past execution across all applications (with an application
filter) - clicking one opens `ExecutionDetailPage`, which reopens that
run's exact historical lineage via the same `LineageFlowView` used for
live viewing. Because event metadata is frozen into `platform.
lineage_events` at record time and each execution's `VersionId` points at
an immutable `ApplicationVersion` snapshot, a later rename of the
application or its screens/components never changes what a past
execution shows - verified with a dedicated integration test that renames
the application after the run and confirms both the execution's stored
name and its component event's metadata still reflect what was true at
run time.

Impact Analysis (Phase 12) is also built: `GET /api/impact/table/{tableId}`
and `GET /api/impact/column/{columnId}` answer the spec's own questions -
which screens/APIs/services use a table, which UI/API/service fields use
a column, which tables reference a table (and which it references) by
foreign key, and which other columns would break if a given column
changed - all derived by walking the same `platform.*` metadata graph
(mappings → components/screens, apis, services, column FKs) rather than
a separate index that could drift. A React Impact Analysis page lets you
pick a table (or one of its columns) and see each answer as a plain list.
Verified with 5 integration tests built on a Customer/Order fixture with
a real FK, covering both directions of the table-reference question and
both empty and non-empty results.

All 12 feature phases from the spec (application management through
impact analysis) are now built and backed by 51 passing xUnit integration
tests run against real PostgreSQL. `lineagestudio-api` and
`lineagestudio-client` are confirmed to build and run correctly under
`docker compose` against the shared `postgres` container (verified
2026-09-06: both images build clean, the API auto-creates `LineageStudioDB`
and answers real requests, the client serves its actual production
`dist/` build).

The spec's own seed demo is built (`Platform.Infrastructure.Persistence.
SeedData`, run automatically in Development unless `Seed:Enabled=false` -
which the integration test host sets, so tests never depend on or fight
over seeded data): a published "Customer Management" application with a
Customer Registration screen, a real `customer(id, name, email, phone)`
table (email `UNIQUE`), a `CustomerService`, `POST /api/customer`, and all
three field mappings - verified live end-to-end including the deliberate
duplicate-email failure case.

`LineageStudio/E2E/` (Playwright, per the spec's own project structure)
drives that seeded demo through a real browser against the real API and
real Postgres - no mocks: the Applications page lists the published seed
app, a full Run → Save → SUCCESS → duplicate-email → FAILED cycle, and a
live "View live lineage" hand-off into the React Flow viewer showing the
actual screen/API nodes from that run. All 3 pass
(`cd LineageStudio/E2E && npm install && npx playwright install --with-deps chromium && npx playwright test`
runs it standalone - it starts both `Platform.Api` and the Vite dev server
itself, reusing anything already listening on their ports). Writing these
also caught a real bug in the Lineage page (it only auto-followed a
*live* future execution, never showed the most recent past one on a
fresh page load) and in `LineageFlowView`'s label derivation (a node's
descriptive label was getting silently dropped once its terminal
COMPLETED event, which carries no metadata, became its "latest" event) -
both are now fixed.

Total: 51 xUnit integration tests + 3 Playwright E2E tests, all green.

A full UI/UX pass followed (2026-09-07): a small design-system layer
(`components/ui` - Button/Input/Select/Textarea/Badge/StatusBadge/Card/
EmptyState/Skeleton/ConfirmDialogHost, `lucide-react` icons and `sonner`
toasts, matching the conventions the other React vertical in this
monorepo, `FlowSphereAI`, already uses) applied across every page:
real toast notifications on every mutation, a styled confirm dialog
replacing `window.confirm`, search/filter on the Applications and
Execution History lists, loading skeletons and empty states everywhere,
a resizable properties panel in the Screen Builder, and a redesigned
sidebar with icons and grouped sections. The Mapping Designer and Lineage
Flow View now auto-layout with **dagre** (left-to-right, per the spec's
"use ELK.js or Dagre" requirement) instead of hand-rolled lane math, and
every React Flow view caps `fitView`'s zoom (`maxZoom: 1`) so small graphs
no longer render with oversized text. Verified with real Playwright
screenshots at each step, which caught two genuine bugs beyond styling:
`fitView` zooming in past 100% on small graphs, and the lineage
visualization rendering a confusing extra node for the backend's internal
`ExecutionCompleted` marker event (fixed by excluding that event type from
the graph) plus Service/Database nodes falling back to a raw GUID label
when their event carried no `name` (fixed by having
`LineageTrackedRuntimeExecutionService` include the real service/table
name in that event's metadata). All 51 integration tests and all 3 E2E
tests still pass after the pass.

`FoodOrderingSystemManagement/` and `PharmacyManagement/` were moved here
from their own top-level folders on 2026-07-07 so the whole platform lives
under one root. Each still has its own `.slnx` if you want to open just that
service in isolation, but `AIO_Systems.slnx` at the root includes everything.

## Prerequisites

- .NET 9 SDK
- Node.js 18+ and npm
- PostgreSQL running locally (default: `localhost:5432`, user `postgres`)

## First-time setup

1. **Databases** — eight separate databases, one per backend (connection
   strings in each project's `appsettings.json`):
   - `SuperAdminDB` — `AIO_Systems/appsettings.json`
   - `FoodOrderDB` — `FoodOrderingSystemManagement/src/FoodOrder.API/appsettings.json`
   - `PharmacyDB` — `PharmacyManagement/Pharmacy.API/appsettings.json`
   - `HRMSDB` — `HRMSManagement/HRMS.API/appsettings.json`
   - `NovaERPDB` — `NovaERP/NovaERP.API/appsettings.json` (only NovaERP
     supports switching provider between PostgreSQL and SQL Server via the
     `DatabaseProvider` config key — the other five are Postgres-only)
   - `WorkflowBuilderDB` — `WorkflowBuilder/Workflow.API/appsettings.json`
   - `FlowSphereAIDB` — `FlowSphereAI/FlowSphere.API/appsettings.json`
   - `ProjectFlowAIDB` — `ProjectFlowAI/ProjectFlowAI.API/appsettings.json`

   No manual table setup needed — EF Core (`EnsureCreated`) creates the
   schema and seeds demo data automatically on first startup for all eight.

2. **Client packages** (each Angular app is independent, install separately;
   FlowSphereAI's and ProjectFlowAI's clients are not Angular, but are
   installed the same way)
   ```powershell
   cd client && npm install                                  # Super Admin UI
   cd FoodOrderingSystemManagement\client && npm install      # Restaurant POS UI
   cd PharmacyManagement\client && npm install                # Pharmacy UI
   cd HRMSManagement\client && npm install                    # HRMS UI
   cd NovaERP\client && npm install                           # NovaERP UI
   cd WorkflowBuilder\client && npm install                   # Workflow Builder UI
   cd FlowSphereAI\client && npm install                      # FlowSphere AI UI
   cd ProjectFlowAI\client && npm install                     # ProjectFlow AI UI
   cd LinkShield\client && npm install                        # LinkShield AI UI
   ```

## Running the project

Start these **in order**, each in its own terminal:

```powershell
# 1. API Gateway — must be up first, everything else routes through it
cd ApiGateway\ApiGateway
dotnet run

# 2. AIO_Systems backend (Super Admin API)
cd AIO_Systems
dotnet run

# 3. FoodOrder.API (Restaurant backend) — only needed if using restaurant features
cd FoodOrderingSystemManagement\src\FoodOrder.API
dotnet run

# 4. Pharmacy.API — only needed if using pharmacy features
cd PharmacyManagement\Pharmacy.API
dotnet run

# 5. HRMS.API — only needed if using HRMS features
cd HRMSManagement\HRMS.API
dotnet run

# 6. NovaERP.API — only needed if using NovaERP features
cd NovaERP\NovaERP.API
dotnet run

# 7. Workflow.API — only needed if using the Workflow Builder
cd WorkflowBuilder\Workflow.API
dotnet run

# 8. FlowSphere.API — only needed if using FlowSphere AI
cd FlowSphereAI\FlowSphere.API
dotnet run

# 9. ProjectFlowAI.API — only needed if using ProjectFlow AI
cd ProjectFlowAI\ProjectFlowAI.API
dotnet run

# 10. LinkShield.API — only needed if using LinkShield AI
cd LinkShield\LinkShield.API
dotnet run

# 11. Whichever client(s) you need
cd client && npm start                              # Super Admin UI -> :4202
cd FoodOrderingSystemManagement\client && npm start  # Restaurant POS -> :4200
cd PharmacyManagement\client && npm start            # Pharmacy UI -> :4201
cd HRMSManagement\client && npm start                # HRMS UI -> :4203
cd NovaERP\client && npm start                       # NovaERP UI -> :4204
cd WorkflowBuilder\client && npm start               # Workflow Builder UI -> :4205
cd FlowSphereAI\client && npm run dev                # FlowSphere AI UI -> :4206
cd ProjectFlowAI\client && npm start                 # ProjectFlow AI UI -> :4207
cd LinkShield\client && npm start                    # LinkShield AI UI -> :4208
```

`LinkShield.API`'s ml-service (optional — see `LinkShield/docs/ml.md`) is not part of this list;
start it separately with `uvicorn` if you need ML predictions (`LinkShield/docs/deployment.md`).

Then open **http://localhost:4202** (Super Admin) or **http://localhost:4200**
(Restaurant POS) in your browser, depending on what you're working on.

### Running multiple backends together in Visual Studio

1. Open `AIO_Systems.slnx` — it now includes every backend project.
2. Right-click the **Solution** → **Configure Startup Projects...**
3. Choose **Multiple startup projects**, set whichever combination of
   `AIO_Systems`, `ApiGateway`, `FoodOrder.API`, `Pharmacy.API`, `HRMS.API`,
   and `NovaERP.API` you need to **Start**.
4. Press **F5**. All selected projects launch together in separate console
   windows.
5. Still start any Angular client(s) separately (`npm start`) — they aren't
   .NET projects and can't join the same Run action.

⚠️ Make sure whichever launch profile you use for **ApiGateway** exposes port
`5000` (check `ApiGateway/ApiGateway/Properties/launchSettings.json`). The
Angular client is hardcoded to call `http://localhost:5000/api` — if the
Gateway comes up on a different port, login will fail with
`ERR_CONNECTION_REFUSED`.

## Default login

```
Email:    superadmin@foodorder.com
Password: SuperAdmin@123
```

This is a SuperAdmin account — no organization, sees every tenant. Change
the password after first login.

### HRMS demo logins

Seeded against the `HRMSDB` database on first run (org: Cloudleap Technologies):

| Role     | Email                  | Password       |
|----------|-------------------------|----------------|
| Admin    | `admin@hrms.local`     | `Admin@123`    |
| HR       | `hr@hrms.local`        | `Hr@12345`     |
| Manager  | `manager@hrms.local`   | `Manager@123`  |
| Employee | `employee@hrms.local`  | `Employee@123` |

### NovaERP demo login

Seeded against the `NovaERPDB` database on first run:

```
Email:    admin@novaerp.local
Password: Admin@123
```

### Workflow Builder demo login

Seeded against the `WorkflowBuilderDB` database on first run:

```
Email:    admin@workflowbuilder.local
Password: Admin@123
```

### FlowSphere AI demo login

Seeded against the `FlowSphereAIDB` database on first run (org: FlowSphere Demo Org):

```
Email:    admin@flowsphere.demo
Password: Admin@123
```

### ProjectFlow AI demo logins

Seeded against the `ProjectFlowAIDB` database on first run (org: ProjectFlow Demo Org, slug
`projectflow-demo`) — one user per system role. `SuperAdmin` is platform-wide (no organization);
every other role belongs to the demo org:

| Role             | Email                          | Password       |
|------------------|---------------------------------|----------------|
| SuperAdmin       | `superadmin@projectflow.local` | `Super@123`    |
| OrganizationAdmin| `admin@projectflow.local`      | `Admin@123`    |
| ProjectManager   | `pm@projectflow.local`         | `Pm@123456`    |
| TeamLead         | `teamlead@projectflow.local`   | `Teamlead@123` |
| Developer        | `dev@projectflow.local`        | `Dev@123456`   |
| QAEngineer       | `qa@projectflow.local`         | `Qa@123456`    |
| BusinessAnalyst  | `ba@projectflow.local`         | `Ba@123456`    |
| Client           | `client@projectflow.local`     | `Client@123`   |
| Guest            | `guest@projectflow.local`      | `Guest@123`    |

ProjectFlow AI Phase 1 covers solution architecture, database design, authentication
(register/login/refresh/logout/verify-email/forgot-and-reset-password/2FA/Google+Microsoft OAuth),
and organization management (organizations/departments/teams/users/roles/permissions/invitations/
audit logs). Tasks/kanban, scrum/gantt, chat/docs, reports, and AI features are later phases and
are intentionally not built yet — see `ProjectFlowAI.Application.Common.PermissionCatalog` for the
reserved permission keys (`projects.*`, `tasks.*`, `reports.*`) those phases will build on, and the
nullable `OrganizationId` FK pattern used throughout the Domain layer.

## Troubleshooting

**Login fails with `ERR_CONNECTION_REFUSED` on port 5000**
Nothing is listening on the Gateway's port yet. Confirm:
- ApiGateway is actually running (check its console window for
  `Now listening on: http://localhost:5000`)
- Its active launch profile maps to port `5000`, not some other port

**Login fails with a 403/401 or "no license assigned"**
The AIO_Systems backend isn't running, or a newly created organization is
missing its Branch/Settings/License rows. New organizations created through
the Super Admin UI auto-provision these — if you hit this on an org created
some other way, it needs manual provisioning.

**A backend process is running but nothing responds**
Occasionally a `dotnet run` process can hang without ever binding to its
port. Check with:
```powershell
netstat -ano | findstr ":5284 :5000"
```
If a PID shows up but the port isn't `LISTENING`, kill that process
(Task Manager, or `Stop-Process -Id <pid> -Force`) and restart it.
