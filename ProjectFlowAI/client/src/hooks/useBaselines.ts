import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ganttApi } from "../api/gantt";
import type { CreateBaselineRequest } from "../types";

export function useBaselines(projectId: string | undefined) {
  return useQuery({
    queryKey: ["baselines", projectId],
    queryFn: () => ganttApi.listBaselines(projectId as string),
    enabled: !!projectId,
    staleTime: 30 * 1000,
  });
}

export function useBaseline(projectId: string | undefined, baselineId: string | undefined) {
  return useQuery({
    queryKey: ["baselines", "detail", projectId, baselineId],
    queryFn: () => ganttApi.getBaseline(projectId as string, baselineId as string),
    enabled: !!projectId && !!baselineId,
    staleTime: 60 * 1000,
  });
}

export function useCreateBaseline(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateBaselineRequest) => ganttApi.createBaseline(projectId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["baselines", projectId] });
    },
  });
}
