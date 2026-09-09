---
sidebar_position: 2
---

# App Shell, Theming & RBAC

The Angular application shell and the permission system that every other feature plugs into.

## What it does

![Roles & Permissions](/img/screenshots/02-roles.png)
*The Roles & Permissions page — assign any combination of permission keys to a custom role.*

- **Shell layout** — a collapsible sidenav with the full feature navigation, a top toolbar with breadcrumb, search box, theme toggle, notification bell, and user menu (`client/src/app/pharmacy-shell/`).
- **Dark/light theme** — a signal-backed `ThemeService` persists the user's preference to `localStorage` (`pharmacy_theme`) and toggles a `dark-mode` class on the document root, driving Angular Material's M3 theme tokens.
- **Dynamic permission catalog** — `Pharmacy.Application/Common/PermissionCatalog.cs` is the single source of truth for every permission key in the system. Each feature module contributes a set of `{module}.{action}` keys (e.g. `medicines.view`, `medicines.edit`). ASP.NET Core authorization policies are registered dynamically from this catalog at startup — adding a module to the catalog automatically creates its policies, no manual policy registration needed.
- **Roles & Permissions UI** — admins can create custom roles and assign any combination of permission keys to them, backed by a `Roles`/`Permissions`/`RolePermissions` many-to-many schema.
- **Client-side gating** — a `PermissionsService` decodes the `permissions` claim out of the JWT on login and exposes `has()`/`hasAny()`. A `*appHasPermission` structural directive hides UI elements (nav items, buttons) the current user isn't allowed to use — this is a UX convenience, not a security boundary; the real enforcement is server-side policies on every endpoint.

## Key files

- Backend: `Pharmacy.Application/Common/PermissionCatalog.cs`, `Pharmacy.Infrastructure/Authorization/` (requirement/handler), `Pharmacy.API/Controllers/RoleController.cs`.
- Frontend: `client/src/app/pharmacy-shell/`, `client/src/app/core/{theme.service,permissions.service,permission.directive}.ts`, `client/src/app/features/roles/`.

## How authentication flows

Login (`Pharmacy.API/Controllers/AuthController.cs`) issues a JWT containing the user's id, org id, branch id, name, role, and a comma-joined list of every permission key their role grants. The Angular client stores this token and decodes claims client-side for routing/UI decisions; every actual API call is re-validated server-side against the JWT's signature and the endpoint's required policy.
