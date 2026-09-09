import type { CalendarEventType } from "../../types";

// Fixed categorical color per event type — assigned by identity, never re-cycled.
export const EVENT_TYPE_COLOR: Record<CalendarEventType, string> = {
  WorkItemDue: "#6355FF",
  Milestone: "#F5A623",
  SprintStart: "#00B8A9",
  SprintEnd: "#E5484D",
};

export const EVENT_TYPE_LABEL: Record<CalendarEventType, string> = {
  WorkItemDue: "Task due",
  Milestone: "Milestone",
  SprintStart: "Sprint start",
  SprintEnd: "Sprint end",
};

// Sequential scale for the workload heatmap — one hue, light -> dark by allocated hours.
export function workloadColor(hours: number, maxHours: number): string {
  if (hours <= 0) return "transparent";
  const ratio = maxHours > 0 ? Math.min(hours / maxHours, 1) : 0;
  // Indigo ramp, light -> dark, matching the app's primary hue.
  const steps = ["#EDEBFF", "#D6D1FF", "#B3AAFF", "#8B7CFF", "#6355FF", "#4A3FCC"];
  const idx = Math.min(Math.floor(ratio * (steps.length - 1)), steps.length - 1);
  return steps[idx];
}
