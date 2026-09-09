import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { automationApi } from "../api/automation";

export const workflowKeys = {
  list: (projectId: string) => ["workflows", "list", projectId] as const,
  detail: (id: string) => ["workflows", "detail", id] as const,
};

export function useWorkflows(projectId: string | undefined) {
  return useQuery({
    queryKey: workflowKeys.list(projectId ?? ""),
    queryFn: () => automationApi.listWorkflows(projectId as string),
    enabled: !!projectId,
    staleTime: 10 * 1000,
  });
}

export function useDeleteWorkflow(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => automationApi.deleteWorkflow(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workflowKeys.list(projectId) }),
  });
}

export function useEnableWorkflow(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => automationApi.enableWorkflow(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workflowKeys.list(projectId) }),
  });
}

export function useDisableWorkflow(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => automationApi.disableWorkflow(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workflowKeys.list(projectId) }),
  });
}
