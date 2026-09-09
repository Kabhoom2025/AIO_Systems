# FlowSphere AI

Enterprise workflow automation module (Power Automate / Zapier / n8n class) built as a new
sibling service inside the `AIO_Systems` monorepo. Phase 1 covers: multi-tenant auth, a
CQRS/MediatR backend, a React Flow workflow designer, an execution engine with 6 node types
(Trigger, HTTP Request, Condition, Delay, Email/SMTP, AI Prompt/OpenAI), and live execution
updates over SignalR.

## Stack

- Backend: .NET 9, Clean Architecture (Domain/Application/Infrastructure/Execution/API), MediatR
  CQRS, EF Core + PostgreSQL, JWT auth (shared secret with sibling services), SignalR.
- Frontend: Next.js (App Router) + TypeScript + Tailwind + shadcn/ui + React Flow + Zustand —
  a deliberate exception to the rest of the platform's Angular clients.

## Ports

- API: `http://localhost:5006`
- Client: `http://localhost:4206`
- Database: `FlowSphereAIDB` (PostgreSQL, `localhost:5432`)

Routed through the shared `ApiGateway` (port 5000) at `/api/flowsphere/*` and
`/hubs/flowsphere/*` — see `ApiGateway/ApiGateway/appsettings.json`.

## Running locally

```powershell
cd FlowSphereAI\FlowSphere.API
dotnet run                    # http://localhost:5006

cd FlowSphereAI\client
npm install
npm run dev                   # http://localhost:4206
```

Demo login: `admin@flowsphere.demo` / `Admin@123` (seeded on first run).

## Running via Docker

From the repo root:

```powershell
docker compose up postgres flowsphere-api flowsphere-client apigateway
```

## Tests

```powershell
cd FlowSphereAI
dotnet test
```

Covers: Clean Architecture layer-dependency rules, MediatR handler-shape rules, workflow
versioning invariants (single-Published), and the execution engine (linear graphs, Condition
branching, node failure/retry).

## What's out of scope in Phase 1

AI Copilot chat/assistant, connector marketplace, billing/subscription, the ~20 connectors
beyond HTTP/SMTP/OpenAI, Kubernetes/Helm/Grafana/Prometheus, and a full test pyramid (no E2E/UI
automation, no load testing yet).
