import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { invitationsApi } from "../api/invitations";
import type { CreateInvitationRequest } from "../types";

export function useInvitations(organizationId: string | undefined) {
  return useQuery({
    queryKey: ["invitations", organizationId],
    queryFn: () => invitationsApi.list(organizationId as string),
    enabled: !!organizationId,
    staleTime: 30 * 1000,
  });
}

export function useCreateInvitation(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateInvitationRequest) => invitationsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invitations", organizationId] });
    },
  });
}

export function useRevokeInvitation(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => invitationsApi.revoke(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invitations", organizationId] });
    },
  });
}
