import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { organizationsApi } from "../api/organizations";
import type { CreateOrganizationRequest, UpdateOrganizationRequest } from "../types";

export const organizationsKeys = {
  all: ["organizations"] as const,
  detail: (id: string) => ["organizations", id] as const,
};

export function useOrganizations() {
  return useQuery({
    queryKey: organizationsKeys.all,
    queryFn: organizationsApi.list,
    staleTime: 60 * 1000,
  });
}

export function useOrganization(id: string | undefined) {
  return useQuery({
    queryKey: organizationsKeys.detail(id ?? ""),
    queryFn: () => organizationsApi.get(id as string),
    enabled: !!id,
  });
}

export function useCreateOrganization() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateOrganizationRequest) => organizationsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: organizationsKeys.all });
    },
  });
}

export function useUpdateOrganization(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateOrganizationRequest) => organizationsApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: organizationsKeys.all });
      queryClient.invalidateQueries({ queryKey: organizationsKeys.detail(id) });
    },
  });
}
