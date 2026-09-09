import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";
import type { CreateWorkItemRequest, UpdateWorkItemRequest, WorkItemListParams } from "../types";

export function useWorkItems(params: WorkItemListParams) {
  return useQuery({
    queryKey: ["workItems", params],
    queryFn: () => workItemsApi.list(params),
    enabled: !!params.projectId,
    placeholderData: (prev) => prev,
    staleTime: 15 * 1000,
  });
}

export function useCreateWorkItem(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateWorkItemRequest) => workItemsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workItems"] });
      queryClient.invalidateQueries({ queryKey: ["kanban", projectId] });
    },
  });
}

export function useUpdateWorkItem(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateWorkItemRequest }) =>
      workItemsApi.update(id, payload),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ["workItems"] });
      queryClient.invalidateQueries({ queryKey: ["workItems", "detail", id] });
      queryClient.invalidateQueries({ queryKey: ["kanban", projectId] });
    },
  });
}

export function useDeleteWorkItem(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => workItemsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workItems"] });
      queryClient.invalidateQueries({ queryKey: ["kanban", projectId] });
    },
  });
}

// Moves a work item into a sprint (or back to the backlog when sprintId is null) via
// POST /work-items/{id}/sprint. Invalidates everything that could show the item's sprint
// membership: the flat work-item lists (backlog), the project kanban, sprint lists/boards,
// and the item's own detail (used by the WorkItemDetailDrawer's sprint picker).
export function useSetWorkItemSprint(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, sprintId }: { id: string; sprintId: string | null }) =>
      workItemsApi.setSprint(id, { sprintId }),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ["workItems"] });
      queryClient.invalidateQueries({ queryKey: ["workItems", "detail", id] });
      queryClient.invalidateQueries({ queryKey: ["kanban", projectId] });
      queryClient.invalidateQueries({ queryKey: ["sprintBoard"] });
      queryClient.invalidateQueries({ queryKey: ["sprints"] });
    },
  });
}
