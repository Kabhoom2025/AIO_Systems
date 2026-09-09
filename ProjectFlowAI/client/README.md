# ProjectFlow AI — Client (Phase 1)

Frontend for ProjectFlow AI, an enterprise project-management service inside the AIO_Systems
monorepo. Phase 1 covers authentication and organization/department/team/user/role/invitation/
audit-log management. Later phases (boards, sprints, gantt, chat, docs, reports, AI features) are
out of scope.

## Tech stack

React 19, TypeScript, Vite, Material UI v6+, TanStack Query v5, React Router v7, Zustand,
React Hook Form + Zod, Axios, Framer Motion (micro-interactions only).

## Getting started

```bash
npm install
npm run dev
```

The dev server runs on **http://localhost:4207** (configured in `vite.config.ts` to match this
repo's port-per-service convention).

All API calls go through the shared API Gateway at `http://localhost:5000`, under the
`/api/projectflow` prefix — configurable via `VITE_API_BASE_URL`.

## Environment variables

Copy `.env.example` to `.env` and fill in as needed:

```bash
cp .env.example .env
```

| Variable | Description |
| --- | --- |
| `VITE_API_BASE_URL` | Base URL for the ProjectFlow AI API (default `http://localhost:5000/api/projectflow`) |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID (Google Identity Services). Leave empty to disable the "Sign in with Google" button. |
| `VITE_MICROSOFT_CLIENT_ID` | Microsoft Entra ID app client ID (MSAL). Leave empty to disable the "Sign in with Microsoft" button. |

## Build

```bash
npm run build
```

Runs `tsc -b` then `vite build`. Output is written to `dist/`.

## Project structure

```
src/
  app/          App.tsx, router.tsx, theme.ts, ProtectedRoute, MSAL config
  api/          axiosClient.ts (auth + silent refresh) + one file per resource
  features/
    auth/       Login, register, forgot/reset password, verify email, 2FA setup/verify
    organization/  Org settings, departments, teams, users, invitations, roles, audit logs
  store/        Zustand stores (auth, theme) — persisted to localStorage
  components/   AppShell, DataTable, ConfirmDialog, EmptyState, Skeletons, form wrappers
  hooks/        TanStack Query hooks per resource
  types/        Shared TypeScript types matching the backend API contract
```
