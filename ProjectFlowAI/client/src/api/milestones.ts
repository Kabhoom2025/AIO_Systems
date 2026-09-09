import { axiosClient } from "./axiosClient";
import type { CreateMilestoneRequest, Milestone, PagedResult, UpdateMilestoneRequest } from "../types";

export const milestonesApi = {
  // The backend exposes milestones as a flat resource filtered by query param (consistent with
  // every other list endpoint in this API, e.g. /departments?organizationId=), not nested under
  // /projects/{id}/milestones, and it returns a PagedResult like every other list endpoint.
  list: (projectId: string) =>
    axiosClient
      .get<PagedResult<Milestone>>(`/milestones`, { params: { projectId, pageSize: 200 } })
      .then((r) => r.data.items),

  create: (payload: CreateMilestoneRequest) =>
    axiosClient.post<Milestone>(`/milestones`, payload).then((r) => r.data),

  update: (id: string, payload: UpdateMilestoneRequest) =>
    axiosClient.put<Milestone>(`/milestones/${id}`, payload).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/milestones/${id}`).then((r) => r.data),
};
