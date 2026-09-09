import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { customFieldsApi } from "../api/customFields";
import type { CreateCustomFieldRequest } from "../types";

export function useCustomFields(projectId: string | undefined) {
  return useQuery({
    queryKey: ["customFields", projectId],
    queryFn: () => customFieldsApi.list(projectId as string),
    enabled: !!projectId,
    staleTime: 60 * 1000,
  });
}

export function useCreateCustomField(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateCustomFieldRequest) => customFieldsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["customFields", projectId] });
    },
  });
}

export function useDeleteCustomField(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customFieldsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["customFields", projectId] });
    },
  });
}

export function useSetCustomFieldValue(workItemId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ definitionId, valueJson }: { definitionId: string; valueJson: string }) =>
      customFieldsApi.setValue(workItemId, definitionId, valueJson),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workItems", "detail", workItemId] });
    },
  });
}
