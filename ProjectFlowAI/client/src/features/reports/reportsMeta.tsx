import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import WarningAmberOutlinedIcon from "@mui/icons-material/WarningAmberOutlined";
import { Chip } from "@mui/material";
import type { ReactNode } from "react";
import type { ProjectHealthStatus } from "../../types";

// Status color follows the app's existing convention (see kanbanMeta's PRIORITY_COLOR):
// success/warning/error carry meaning and always ship with an icon + label, never color alone.
export const HEALTH_COLOR: Record<ProjectHealthStatus, "success" | "warning" | "error"> = {
  Green: "success",
  Yellow: "warning",
  Red: "error",
};

export const HEALTH_ICON: Record<ProjectHealthStatus, ReactNode> = {
  Green: <CheckCircleOutlineIcon fontSize="small" />,
  Yellow: <WarningAmberOutlinedIcon fontSize="small" />,
  Red: <ErrorOutlineIcon fontSize="small" />,
};

export const HEALTH_LABEL: Record<ProjectHealthStatus, string> = {
  Green: "Healthy",
  Yellow: "At risk",
  Red: "Critical",
};

interface HealthChipProps {
  health: ProjectHealthStatus;
  size?: "small" | "medium";
}

export function HealthChip({ health, size = "small" }: HealthChipProps) {
  return (
    <Chip
      size={size}
      color={HEALTH_COLOR[health]}
      icon={HEALTH_ICON[health] as React.ReactElement}
      label={HEALTH_LABEL[health]}
      variant="outlined"
    />
  );
}

/** Formats a utilization percent that may be null (e.g. no estimate to compare against)
 * as "—" rather than a bare "NaN%" or misleading "0%". */
export function formatPercent(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return "—";
  return `${Math.round(value)}%`;
}

export function formatHours(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return "—";
  return value.toFixed(1);
}

export function formatCurrency(value: number | null | undefined, hasRate: boolean): string {
  if (value === null || value === undefined || Number.isNaN(value)) return "—";
  const formatted = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(
    value
  );
  return hasRate ? formatted : `${formatted} (no rate configured)`;
}
