import { useQuery } from "@tanstack/react-query";
import { permissionsApi } from "../api/permissions";

export function usePermissions() {
  return useQuery({
    queryKey: ["permissions"],
    queryFn: permissionsApi.list,
    staleTime: 5 * 60 * 1000,
  });
}
