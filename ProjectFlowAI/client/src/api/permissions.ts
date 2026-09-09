import { axiosClient } from "./axiosClient";
import type { Permission } from "../types";

export const permissionsApi = {
  list: () => axiosClient.get<Permission[]>("/permissions").then((r) => r.data),
};
