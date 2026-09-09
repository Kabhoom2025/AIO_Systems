import { axiosClient } from "./axiosClient";
import type { AddTeamMemberRequest, CreateTeamRequest, PagedResult, Team, UpdateTeamRequest } from "../types";

export const teamsApi = {
  // Backend returns a PagedResult, not a flat array.
  list: (organizationId: string, departmentId?: string) =>
    axiosClient
      .get<PagedResult<Team>>("/teams", { params: { organizationId, departmentId, pageSize: 500 } })
      .then((r) => r.data.items),

  get: (id: string) => axiosClient.get<Team>(`/teams/${id}`).then((r) => r.data),

  create: (payload: CreateTeamRequest) =>
    axiosClient.post<Team>("/teams", payload).then((r) => r.data),

  update: (id: string, payload: UpdateTeamRequest) =>
    axiosClient.put<Team>(`/teams/${id}`, payload).then((r) => r.data),

  addMember: (teamId: string, payload: AddTeamMemberRequest) =>
    axiosClient.post<void>(`/teams/${teamId}/members`, payload).then((r) => r.data),

  removeMember: (teamId: string, userId: string) =>
    axiosClient.delete<void>(`/teams/${teamId}/members/${userId}`).then((r) => r.data),
};
