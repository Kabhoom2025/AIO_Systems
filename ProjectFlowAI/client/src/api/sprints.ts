import { axiosClient } from "./axiosClient";
import { toKanbanCard, type RawKanbanColumn } from "./workItems";
import type {
  BurndownData,
  BurnupData,
  CreateRetrospectiveNoteRequest,
  CreateSprintRequest,
  PagedResult,
  RetrospectiveNote,
  RetrospectiveNotesResponse,
  Sprint,
  SprintBoard,
  SprintListParams,
  UpdateSprintRequest,
  VelocityData,
} from "../types";

interface RawSprintBoard {
  sprintId: string;
  columns: RawKanbanColumn[];
}

function toSprintBoard(dto: RawSprintBoard): SprintBoard {
  return {
    sprintId: dto.sprintId,
    columns: dto.columns.map((col) => ({
      status: col.status,
      items: col.items.map(toKanbanCard),
    })),
  };
}

export const sprintsApi = {
  list: (params: SprintListParams) =>
    axiosClient.get<PagedResult<Sprint>>("/sprints", { params }).then((r) => r.data),

  get: (id: string) => axiosClient.get<Sprint>(`/sprints/${id}`).then((r) => r.data),

  create: (payload: CreateSprintRequest) =>
    axiosClient.post<Sprint>("/sprints", payload).then((r) => r.data),

  update: (id: string, payload: UpdateSprintRequest) =>
    axiosClient.put<Sprint>(`/sprints/${id}`, payload).then((r) => r.data),

  start: (id: string) => axiosClient.post<Sprint>(`/sprints/${id}/start`).then((r) => r.data),

  complete: (id: string) =>
    axiosClient.post<Sprint>(`/sprints/${id}/complete`).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/sprints/${id}`).then((r) => r.data),

  getBoard: (id: string) =>
    axiosClient.get<RawSprintBoard>(`/sprints/${id}/board`).then((r) => toSprintBoard(r.data)),

  getVelocity: (projectId: string) =>
    axiosClient.get<VelocityData>(`/projects/${projectId}/velocity`).then((r) => r.data),

  getBurndown: (id: string) =>
    axiosClient.get<BurndownData>(`/sprints/${id}/burndown`).then((r) => r.data),

  getBurnup: (id: string) =>
    axiosClient.get<BurnupData>(`/sprints/${id}/burnup`).then((r) => r.data),

  getRetrospective: (id: string) =>
    axiosClient.get<RetrospectiveNotesResponse>(`/sprints/${id}/retrospective`).then((r) => r.data),

  addRetrospectiveNote: (id: string, payload: CreateRetrospectiveNoteRequest) =>
    axiosClient
      .post<RetrospectiveNote>(`/sprints/${id}/retrospective/notes`, payload)
      .then((r) => r.data),

  // Top-level route — not nested under /sprints/{id}, per contract.
  removeRetrospectiveNote: (noteId: string) =>
    axiosClient.delete<void>(`/retrospective-notes/${noteId}`).then((r) => r.data),
};
