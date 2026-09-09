import { axiosClient } from "./axiosClient";
import type {
  AddProjectMemberRequest,
  CreateProjectRequest,
  PagedResult,
  Project,
  ProjectDetail,
  ProjectListParams,
  ProjectMember,
  UpdateProjectRequest,
} from "../types";

export const projectsApi = {
  list: (params: ProjectListParams) =>
    axiosClient.get<PagedResult<Project>>("/projects", { params }).then((r) => r.data),

  get: (id: string) => axiosClient.get<ProjectDetail>(`/projects/${id}`).then((r) => r.data),

  create: (payload: CreateProjectRequest) =>
    axiosClient.post<Project>("/projects", payload).then((r) => r.data),

  update: (id: string, payload: UpdateProjectRequest) =>
    axiosClient.put<Project>(`/projects/${id}`, payload).then((r) => r.data),

  archive: (id: string) => axiosClient.post<void>(`/projects/${id}/archive`).then((r) => r.data),

  listMembers: (id: string) =>
    axiosClient.get<ProjectMember[]>(`/projects/${id}/members`).then((r) => r.data),

  addMember: (id: string, payload: AddProjectMemberRequest) =>
    axiosClient.post<void>(`/projects/${id}/members`, payload).then((r) => r.data),

  removeMember: (id: string, userId: string) =>
    axiosClient.delete<void>(`/projects/${id}/members/${userId}`).then((r) => r.data),
};
