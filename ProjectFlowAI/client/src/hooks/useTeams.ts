import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { teamsApi } from "../api/teams";
import type { AddTeamMemberRequest, CreateTeamRequest, UpdateTeamRequest } from "../types";

export const teamsKeys = {
  all: (organizationId: string, departmentId?: string) =>
    ["teams", organizationId, departmentId ?? "all"] as const,
};

export function useTeams(organizationId: string | undefined, departmentId?: string) {
  return useQuery({
    queryKey: teamsKeys.all(organizationId ?? "", departmentId),
    queryFn: () => teamsApi.list(organizationId as string, departmentId),
    enabled: !!organizationId,
    staleTime: 60 * 1000,
  });
}

export function useCreateTeam(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateTeamRequest) => teamsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams", organizationId] });
    },
  });
}

export function useUpdateTeam(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateTeamRequest }) =>
      teamsApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams", organizationId] });
    },
  });
}

export function useAddTeamMember(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ teamId, payload }: { teamId: string; payload: AddTeamMemberRequest }) =>
      teamsApi.addMember(teamId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams", organizationId] });
    },
  });
}

export function useRemoveTeamMember(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ teamId, userId }: { teamId: string; userId: string }) =>
      teamsApi.removeMember(teamId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams", organizationId] });
    },
  });
}
