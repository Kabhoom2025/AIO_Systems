import { axiosClient } from "./axiosClient";
import { STATUS_TO_KANBAN_COLUMN } from "../types";
import type {
  ChecklistItem,
  CreateCommentRequest,
  CreateDependencyRequest,
  CreateTimeLogRequest,
  CreateWorkItemRequest,
  KanbanBoard,
  KanbanCard,
  MoveWorkItemRequest,
  MoveWorkItemToSprintRequest,
  PagedResult,
  UpdateCommentRequest,
  UpdateWorkItemRequest,
  WorkItem,
  WorkItemAttachment,
  WorkItemComment,
  WorkItemDependency,
  WorkItemDetail,
  WorkItemFollower,
  WorkItemListParams,
  WorkItemPriority,
  WorkItemStatus,
  WorkItemTimeLog,
  WorkItemType,
} from "../types";

// Raw shape of WorkItemDto as returned by the API (backend/DTOs.cs) — not the same shape as
// the frontend's KanbanCard, which is a UI-oriented projection built by toKanbanCard() below.
export interface RawKanbanWorkItem {
  id: string;
  projectId: string;
  title: string;
  status: WorkItemStatus;
  priority: WorkItemPriority;
  type: WorkItemType;
  storyPoints?: number | null;
  assigneeUserId?: string | null;
  assigneeFullName?: string | null;
  dueDate?: string | null;
  position: number;
  labels: { id: string; name: string; colorHex: string }[];
}

export interface RawKanbanColumn {
  status: WorkItemStatus;
  items: RawKanbanWorkItem[];
}

export interface RawKanbanBoard {
  projectId: string;
  columns: RawKanbanColumn[];
}

// The board/sprint-board endpoints don't return checklist/comment counts (that data only comes
// back on the single work-item detail endpoint), so those fields default to 0 here.
export function toKanbanCard(w: RawKanbanWorkItem): KanbanCard {
  return {
    id: w.id,
    title: w.title,
    priority: w.priority,
    type: w.type,
    storyPoints: w.storyPoints,
    assigneeUserId: w.assigneeUserId,
    assigneeName: w.assigneeFullName,
    labelColors: (w.labels ?? []).map((l) => l.colorHex),
    checklistDone: 0,
    checklistTotal: 0,
    commentCount: 0,
    dueDate: w.dueDate,
    position: w.position,
  };
}

function toKanbanBoard(dto: RawKanbanBoard): KanbanBoard {
  const board: KanbanBoard = {
    backlog: [],
    todo: [],
    inProgress: [],
    codeReview: [],
    testing: [],
    blocked: [],
    done: [],
  };
  for (const col of dto.columns) {
    board[STATUS_TO_KANBAN_COLUMN[col.status]] = col.items.map(toKanbanCard);
  }
  return board;
}

export const workItemsApi = {
  list: (params: WorkItemListParams) =>
    axiosClient.get<PagedResult<WorkItem>>("/work-items", { params }).then((r) => r.data),

  get: (id: string) => axiosClient.get<WorkItemDetail>(`/work-items/${id}`).then((r) => r.data),

  getKanban: (projectId: string) =>
    axiosClient
      .get<RawKanbanBoard>(`/projects/${projectId}/kanban`)
      .then((r) => toKanbanBoard(r.data)),

  create: (payload: CreateWorkItemRequest) =>
    axiosClient.post<WorkItem>("/work-items", payload).then((r) => r.data),

  update: (id: string, payload: UpdateWorkItemRequest) =>
    axiosClient.put<WorkItem>(`/work-items/${id}`, payload).then((r) => r.data),

  move: (id: string, payload: MoveWorkItemRequest) =>
    axiosClient.post<void>(`/work-items/${id}/move`, payload).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/work-items/${id}`).then((r) => r.data),

  // Checklist
  addChecklistItem: (id: string, text: string) =>
    axiosClient.post<ChecklistItem>(`/work-items/${id}/checklist`, { text }).then((r) => r.data),

  toggleChecklistItem: (id: string, itemId: string, isDone: boolean) =>
    axiosClient
      .patch<ChecklistItem>(`/work-items/${id}/checklist/${itemId}`, { isDone })
      .then((r) => r.data),

  removeChecklistItem: (id: string, itemId: string) =>
    axiosClient.delete<void>(`/work-items/${id}/checklist/${itemId}`).then((r) => r.data),

  // Labels on a work item
  addLabel: (id: string, labelId: string) =>
    axiosClient.post<void>(`/work-items/${id}/labels`, { labelId }).then((r) => r.data),

  removeLabel: (id: string, labelId: string) =>
    axiosClient.delete<void>(`/work-items/${id}/labels/${labelId}`).then((r) => r.data),

  // Followers — the backend takes userId in the JSON body on add, but in the URL on remove
  addFollower: (id: string, userId: string) =>
    axiosClient
      .post<WorkItemFollower>(`/work-items/${id}/followers`, { userId })
      .then((r) => r.data),

  removeFollower: (id: string, userId: string) =>
    axiosClient.delete<void>(`/work-items/${id}/followers/${userId}`).then((r) => r.data),

  // Dependencies
  addDependency: (id: string, payload: CreateDependencyRequest) =>
    axiosClient
      .post<WorkItemDependency>(`/work-items/${id}/dependencies`, payload)
      .then((r) => r.data),

  // Removal is a top-level /work-items/dependencies/{dependencyId} route, not nested under the
  // owning work item's id.
  removeDependency: (_id: string, depId: string) =>
    axiosClient.delete<void>(`/work-items/dependencies/${depId}`).then((r) => r.data),

  // Comments — update/delete are /work-items/comments/{commentId}, not a top-level /comments
  // resource (there is no separate CommentsController in this API).
  addComment: (id: string, payload: CreateCommentRequest) =>
    axiosClient.post<WorkItemComment>(`/work-items/${id}/comments`, payload).then((r) => r.data),

  updateComment: (commentId: string, payload: UpdateCommentRequest) =>
    axiosClient.put<WorkItemComment>(`/work-items/comments/${commentId}`, payload).then((r) => r.data),

  removeComment: (commentId: string) =>
    axiosClient.delete<void>(`/work-items/comments/${commentId}`).then((r) => r.data),

  // Attachments
  uploadAttachment: (id: string, file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return axiosClient
      .post<WorkItemAttachment>(`/work-items/${id}/attachments`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      .then((r) => r.data);
  },

  // Deletion is /work-items/attachments/{attachmentId} — there's no top-level /attachments
  // resource for delete (only a separate AttachmentsController for GET .../download).
  removeAttachment: (attachmentId: string) =>
    axiosClient.delete<void>(`/work-items/attachments/${attachmentId}`).then((r) => r.data),

  // Time logs
  addTimeLog: (id: string, payload: CreateTimeLogRequest) =>
    axiosClient.post<WorkItemTimeLog>(`/work-items/${id}/time-logs`, payload).then((r) => r.data),

  removeTimeLog: (timeLogId: string) =>
    axiosClient.delete<void>(`/work-items/time-logs/${timeLogId}`).then((r) => r.data),

  setTimeLogBillable: (timeLogId: string, isBillable: boolean) =>
    axiosClient
      .patch<WorkItemTimeLog>(`/work-items/time-logs/${timeLogId}/billable`, { isBillable })
      .then((r) => r.data),

  // Sprint assignment — lives on the existing work-items resource, not a new sprints endpoint.
  // sprintId: null moves the item back to the backlog.
  setSprint: (id: string, payload: MoveWorkItemToSprintRequest) =>
    axiosClient.post<WorkItem>(`/work-items/${id}/sprint`, payload).then((r) => r.data),
};
