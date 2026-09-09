import BugReportOutlinedIcon from "@mui/icons-material/BugReportOutlined";
import AutoAwesomeMotionOutlinedIcon from "@mui/icons-material/AutoAwesomeMotionOutlined";
import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";
import BookmarkOutlinedIcon from "@mui/icons-material/BookmarkOutlined";
import type { ReactNode } from "react";
import type { KanbanBoard, WorkItemPriority, WorkItemType } from "../../types";

export const KANBAN_COLUMNS: { key: keyof KanbanBoard; label: string }[] = [
  { key: "backlog", label: "Backlog" },
  { key: "todo", label: "To Do" },
  { key: "inProgress", label: "In Progress" },
  { key: "codeReview", label: "Code Review" },
  { key: "testing", label: "Testing" },
  { key: "blocked", label: "Blocked" },
  { key: "done", label: "Done" },
];

export const PRIORITY_COLOR: Record<
  WorkItemPriority,
  "default" | "info" | "warning" | "error" | "success"
> = {
  Lowest: "default",
  Low: "info",
  Medium: "warning",
  High: "error",
  Highest: "error",
};

export const TYPE_ICON: Record<WorkItemType, ReactNode> = {
  Task: <AssignmentOutlinedIcon fontSize="small" />,
  Bug: <BugReportOutlinedIcon fontSize="small" color="error" />,
  Story: <BookmarkOutlinedIcon fontSize="small" color="success" />,
  Epic: <AutoAwesomeMotionOutlinedIcon fontSize="small" color="secondary" />,
};
