import type { SprintStatus } from "../../types";

export const SPRINT_STATUS_COLOR: Record<
  SprintStatus,
  "default" | "info" | "warning" | "success"
> = {
  Planned: "default",
  Active: "info",
  Completed: "success",
};
