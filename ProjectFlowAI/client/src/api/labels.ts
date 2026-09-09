import { axiosClient } from "./axiosClient";
import type { CreateLabelRequest, Label } from "../types";

export const labelsApi = {
  list: (projectId: string) =>
    axiosClient.get<Label[]>(`/labels`, { params: { projectId } }).then((r) => r.data),

  create: (payload: CreateLabelRequest) =>
    axiosClient.post<Label>("/labels", payload).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/labels/${id}`).then((r) => r.data),
};
