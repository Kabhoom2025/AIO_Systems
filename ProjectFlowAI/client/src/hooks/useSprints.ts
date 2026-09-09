import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { sprintsApi } from "../api/sprints";
import type { CreateSprintRequest, SprintListParams, UpdateSprintRequest } from "../types";

export function sprintsKey(params: SprintListParams) {
  return ["sprints", params] as const;
}

export function useSprints(params: SprintListParams) {
  return useQuery({
    queryKey: sprintsKey(params),
    queryFn: () => sprintsApi.list(params),
    enabled: !!params.projectId,
    placeholderData: (prev) => prev,
    staleTime: 15 * 1000,
  });
}

export function useSprint(id: string | undefined) {
  return useQuery({
    queryKey: ["sprints", "detail", id],
    queryFn: () => sprintsApi.get(id as string),
    enabled: !!id,
    staleTime: 15 * 1000,
  });
}

function invalidateSprintLists(queryClient: ReturnType<typeof useQueryClient>, projectId: string) {
  queryClient.invalidateQueries({ queryKey: ["sprints"], predicate: (q) => {
    const params = q.queryKey[1] as SprintListParams | undefined;
    return q.queryKey[0] === "sprints" && (!params || params.projectId === projectId);
  } });
}

export function useCreateSprint(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateSprintRequest) => sprintsApi.create(payload),
    onSuccess: () => invalidateSprintLists(queryClient, projectId),
  });
}

export function useUpdateSprint(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateSprintRequest }) =>
      sprintsApi.update(id, payload),
    onSuccess: (_, { id }) => {
      invalidateSprintLists(queryClient, projectId);
      queryClient.invalidateQueries({ queryKey: ["sprints", "detail", id] });
    },
  });
}

export function useStartSprint(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => sprintsApi.start(id),
    onSuccess: (_, id) => {
      invalidateSprintLists(queryClient, projectId);
      queryClient.invalidateQueries({ queryKey: ["sprints", "detail", id] });
    },
  });
}

export function useCompleteSprint(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => sprintsApi.complete(id),
    onSuccess: (_, id) => {
      invalidateSprintLists(queryClient, projectId);
      queryClient.invalidateQueries({ queryKey: ["sprints", "detail", id] });
      queryClient.invalidateQueries({ queryKey: ["velocity", projectId] });
    },
  });
}

export function useDeleteSprint(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => sprintsApi.remove(id),
    onSuccess: () => invalidateSprintLists(queryClient, projectId),
  });
}
