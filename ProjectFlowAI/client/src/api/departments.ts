import { axiosClient } from "./axiosClient";
import type { CreateDepartmentRequest, Department, PagedResult, UpdateDepartmentRequest } from "../types";

export const departmentsApi = {
  // Backend returns a PagedResult, not a flat array — request a large page since
  // this screen has no pagination UI of its own.
  list: (organizationId: string) =>
    axiosClient
      .get<PagedResult<Department>>("/departments", { params: { organizationId, pageSize: 500 } })
      .then((r) => r.data.items),

  get: (id: string) =>
    axiosClient.get<Department>(`/departments/${id}`).then((r) => r.data),

  create: (payload: CreateDepartmentRequest) =>
    axiosClient.post<Department>("/departments", payload).then((r) => r.data),

  update: (id: string, payload: UpdateDepartmentRequest) =>
    axiosClient.put<Department>(`/departments/${id}`, payload).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/departments/${id}`).then((r) => r.data),
};
