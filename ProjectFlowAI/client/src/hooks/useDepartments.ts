import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { departmentsApi } from "../api/departments";
import type { CreateDepartmentRequest, UpdateDepartmentRequest } from "../types";

export const departmentsKeys = {
  all: (organizationId: string) => ["departments", organizationId] as const,
};

export function useDepartments(organizationId: string | undefined) {
  return useQuery({
    queryKey: departmentsKeys.all(organizationId ?? ""),
    queryFn: () => departmentsApi.list(organizationId as string),
    enabled: !!organizationId,
    staleTime: 60 * 1000,
  });
}

export function useCreateDepartment(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateDepartmentRequest) => departmentsApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: departmentsKeys.all(organizationId) });
    },
  });
}

export function useUpdateDepartment(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateDepartmentRequest }) =>
      departmentsApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: departmentsKeys.all(organizationId) });
    },
  });
}

export function useDeleteDepartment(organizationId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => departmentsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: departmentsKeys.all(organizationId) });
    },
  });
}
