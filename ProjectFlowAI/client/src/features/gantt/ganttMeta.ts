import type { WorkItemStatus } from "../../types";

export type GanttZoom = "day" | "week" | "month";

export const ZOOM_PX_PER_DAY: Record<GanttZoom, number> = {
  day: 40,
  week: 16,
  month: 6,
};

export const ROW_HEIGHT = 40;
export const HEADER_HEIGHT = 48;
export const LABEL_COLUMN_WIDTH = 260;

// Status color ramp — sequential-ish progression from "not started" to "done", plus a
// distinct blocked/error color. Kept in step with the app's indigo/violet theme rather than
// a generic rainbow.
export const STATUS_BAR_COLOR: Record<WorkItemStatus, string> = {
  Backlog: "#9AA0B4",
  ToDo: "#7C89F5",
  InProgress: "#6355FF",
  CodeReview: "#3DA8FF",
  Testing: "#00B8A9",
  Blocked: "#E5484D",
  Done: "#2FAE60",
};

export const CRITICAL_PATH_COLOR = "#E5484D";
