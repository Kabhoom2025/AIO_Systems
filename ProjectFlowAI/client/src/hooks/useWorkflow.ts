import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { automationApi } from "../api/automation";
import { workflowKeys } from "./useWorkflows";
import type { CreateWorkflowRequest, UpdateWorkflowRequest } from "../types";

export function useWorkflow(id: string | undefined) {
  return useQuery({
    queryKey: workflowKeys.detail(id ?? ""),
    queryFn: () => automationApi.getWorkflow(id as string),
    enabled: !!id,
  });
}

export function useCreateWorkflow() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateWorkflowRequest) => automationApi.createWorkflow(payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: workflowKeys.list(data.projectId) });
    },
  });
}

export function useUpdateWorkflow(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateWorkflowRequest) => automationApi.updateWorkflow(id, payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: workflowKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: workflowKeys.list(data.projectId) });
    },
  });
}
