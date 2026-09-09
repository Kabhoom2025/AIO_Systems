import { axiosClient } from "./axiosClient";
import type { CreateCustomFieldRequest, CustomFieldDefinition } from "../types";

export const customFieldsApi = {
  list: (projectId: string) =>
    axiosClient
      .get<CustomFieldDefinition[]>(`/custom-fields`, { params: { projectId } })
      .then((r) => r.data),

  create: (payload: CreateCustomFieldRequest) =>
    axiosClient.post<CustomFieldDefinition>("/custom-fields", payload).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/custom-fields/${id}`).then((r) => r.data),

  setValue: (workItemId: string, definitionId: string, valueJson: string) =>
    axiosClient
      .post<void>(`/work-items/${workItemId}/custom-field-values`, {
        customFieldDefinitionId: definitionId,
        valueJson,
      })
      .then((r) => r.data),
};
