import { axiosClient } from "./axiosClient";
import type { PagedResult, User, UserListParams } from "../types";

export const usersApi = {
  list: (params: UserListParams) =>
    axiosClient.get<PagedResult<User>>("/users", { params }).then((r) => r.data),

  get: (id: string) => axiosClient.get<User>(`/users/${id}`).then((r) => r.data),
};
