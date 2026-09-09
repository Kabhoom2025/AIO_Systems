---
sidebar_position: 13
---

# Settings

Organization profile management and self-service account settings — the two concrete things "Settings" means for this system.

## What it does

![Organization settings](/img/screenshots/13-settings-org.png)
*The Organization tab — profile fields plus currency, GST number, and invoice prefix.*

![My Profile](/img/screenshots/13-settings-profile.png)
*My Profile — account info and the self-service change-password form.*

- **Organization tab** — edit the pharmacy's profile: name, address, phone, email, license number, plus three new business-config fields added in this phase (currency, GST number, invoice number prefix). Editable only by users with `settings.edit`; visible read-only to anyone with `settings.view`.
- **My Profile tab** — every logged-in user (regardless of permissions) can view their own name/email/role/organization and change their own password, via a current-password + new-password form.

## Key files

- Backend: new `Currency`/`GstNumber`/`InvoiceNumberPrefix` fields on `Pharmacy.Domain/Entities/Organization.cs`, `Pharmacy.API/Controllers/OrganizationController.cs`, and a `ChangePasswordAsync` method added to `Pharmacy.Infrastructure/Authentication/AuthService.cs` (kept there rather than on `UserService`, since it needs the `PasswordHasher` utility that only Infrastructure has direct access to, and Application-layer code must not depend on Infrastructure).
- Frontend: `client/src/app/features/settings/`.

## Scope

No duplicate branch management (Branches already has its own full CRUD page), no system-wide feature-flags/integrations screen, and change-password is self-service only — there's no admin-resets-another-user's-password flow, since that belongs with Users/Roles management, not personal Settings. The new org fields (currency, GST number, invoice prefix) are stored and editable but not yet wired into other modules — e.g. Reports still shows a hardcoded ₹, invoices don't yet use the configurable prefix.
