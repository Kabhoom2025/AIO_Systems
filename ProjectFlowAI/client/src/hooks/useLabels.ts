import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { labelsApi } from "../api/labels";
import type { CreateLabelRequest } from "../types";

export function useLabels(projectId: string | undefined) {
  return useQuery({
    queryKey: ["labels", projectId],
    queryFn: () => labelsApi.list(projectId as string),
    enabled: !!projectId,
    staleTime: 60 * 1000,
  });
}

export function useCreateLabel(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateLabelRequest) => labelsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["labels", projectId] });
    },
  });
}

export function useDeleteLabel(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => labelsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["labels", projectId] });
    },
  });
}
