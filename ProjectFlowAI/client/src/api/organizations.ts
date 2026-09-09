import { axiosClient } from "./axiosClient";
import type { CreateOrganizationRequest, Organization, PagedResult, UpdateOrganizationRequest } from "../types";

export const organizationsApi = {
  // Backend returns a PagedResult, not a flat array.
  list: () =>
    axiosClient
      .get<PagedResult<Organization>>("/organizations", { params: { pageSize: 500 } })
      .then((r) => r.data.items),

  get: (id: string) =>
    axiosClient.get<Organization>(`/organizations/${id}`).then((r) => r.data),

  create: (payload: CreateOrganizationRequest) =>
    axiosClient.post<Organization>("/organizations", payload).then((r) => r.data),

  update: (id: string, payload: UpdateOrganizationRequest) =>
    axiosClient.put<Organization>(`/organizations/${id}`, payload).then((r) => r.data),
};
