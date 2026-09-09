import { useQuery } from "@tanstack/react-query";
import { usersApi } from "../api/users";
import type { UserListParams } from "../types";

export function useUsers(params: UserListParams) {
  return useQuery({
    queryKey: ["users", params],
    queryFn: () => usersApi.list(params),
    enabled: !!params.organizationId,
    placeholderData: (prev) => prev,
    staleTime: 30 * 1000,
  });
}

export function useUser(id: string | undefined) {
  return useQuery({
    queryKey: ["users", "detail", id],
    queryFn: () => usersApi.get(id as string),
    enabled: !!id,
  });
}
