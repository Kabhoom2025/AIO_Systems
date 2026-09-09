import BusinessOutlinedIcon from "@mui/icons-material/BusinessOutlined";
import { AppShell } from "./AppShell";
import { EmptyState } from "./EmptyState";

/**
 * SuperAdmin accounts are platform-wide and belong to no organization by design (they manage
 * organizations themselves, not day-to-day resources inside one). Every org-scoped page —
 * Departments, Teams, Roles, Invitations, Users, Audit Logs — must show this instead of silently
 * sending an empty organizationId to the API, which the backend correctly rejects as a malformed
 * request rather than an empty result.
 */
export function NoOrganizationState() {
  return (
    <AppShell>
      <EmptyState
        icon={<BusinessOutlinedIcon sx={{ fontSize: 48 }} />}
        title="No organization"
        description="This account isn't a member of any organization, so there's nothing here to manage. Log in as a user who belongs to an organization, or use the Organizations panel to manage tenants directly."
      />
    </AppShell>
  );
}
