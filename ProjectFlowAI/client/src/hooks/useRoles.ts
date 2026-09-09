import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { rolesApi } from "../api/roles";
import type { CreateRoleRequest } from "../types";

export function useRoles(organizationId: string | undefined) {
  return useQuery({
    queryKey: ["roles", organizationId],
    queryFn: () => rolesApi.list(organizationId as string),
    enabled: !!organizationId,
    staleTime: 60 * 1000,
  });
}

export function useCreateRole(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateRoleRequest) => rolesApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["roles", organizationId] });
    },
  });
}
