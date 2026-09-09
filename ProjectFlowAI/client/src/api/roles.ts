import { axiosClient } from "./axiosClient";
import type { CreateRoleRequest, PagedResult, Role } from "../types";

export const rolesApi = {
  // Backend returns a PagedResult, not a flat array.
  list: (organizationId: string) =>
    axiosClient
      .get<PagedResult<Role>>("/roles", { params: { organizationId, pageSize: 500 } })
      .then((r) => r.data.items),

  create: (payload: CreateRoleRequest) =>
    axiosClient.post<Role>("/roles", payload).then((r) => r.data),
};
