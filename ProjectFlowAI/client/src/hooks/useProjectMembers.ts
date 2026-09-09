import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { projectsApi } from "../api/projects";
import type { AddProjectMemberRequest } from "../types";

export function useProjectMembers(projectId: string | undefined) {
  return useQuery({
    queryKey: ["projectMembers", projectId],
    queryFn: () => projectsApi.listMembers(projectId as string),
    enabled: !!projectId,
    staleTime: 30 * 1000,
  });
}

export function useAddProjectMember(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: AddProjectMemberRequest) => projectsApi.addMember(projectId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projectMembers", projectId] });
      queryClient.invalidateQueries({ queryKey: ["projects", "detail", projectId] });
    },
  });
}

export function useRemoveProjectMember(projectId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => projectsApi.removeMember(projectId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projectMembers", projectId] });
      queryClient.invalidateQueries({ queryKey: ["projects", "detail", projectId] });
    },
  });
}
