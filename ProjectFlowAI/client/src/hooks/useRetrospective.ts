import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { sprintsApi } from "../api/sprints";
import type { CreateRetrospectiveNoteRequest } from "../types";

export function retrospectiveKey(sprintId: string) {
  return ["retrospective", sprintId] as const;
}

export function useRetrospective(sprintId: string | undefined) {
  return useQuery({
    queryKey: retrospectiveKey(sprintId ?? ""),
    queryFn: () => sprintsApi.getRetrospective(sprintId as string),
    enabled: !!sprintId,
    staleTime: 15 * 1000,
  });
}

export function useAddRetrospectiveNote(sprintId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateRetrospectiveNoteRequest) =>
      sprintsApi.addRetrospectiveNote(sprintId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: retrospectiveKey(sprintId) });
    },
  });
}

export function useDeleteRetrospectiveNote(sprintId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (noteId: string) => sprintsApi.removeRetrospectiveNote(noteId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: retrospectiveKey(sprintId) });
    },
  });
}
