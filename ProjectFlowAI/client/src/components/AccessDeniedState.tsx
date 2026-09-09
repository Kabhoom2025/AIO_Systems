import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import { EmptyState } from "./EmptyState";

interface AccessDeniedStateProps {
  description?: string;
}

/** Shown in place of a raw 403 error dump wherever a report/screen requires a
 * permission (e.g. reports.view) the current user's role doesn't grant. */
export function AccessDeniedState({
  description = "You don't have permission to view this. Ask an organization admin for access.",
}: AccessDeniedStateProps) {
  return (
    <EmptyState
      icon={<LockOutlinedIcon sx={{ fontSize: 48 }} />}
      title="You don't have access to this report"
      description={description}
    />
  );
}
