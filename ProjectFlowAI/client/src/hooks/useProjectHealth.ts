import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import { retrySkipping403 } from "../utils/apiErrors";

export function useProjectHealth(projectId: string | undefined) {
  return useQuery({
    queryKey: ["reports", "project-health", projectId],
    queryFn: () => reportsApi.getProjectHealth(projectId as string),
    enabled: !!projectId,
    staleTime: 30 * 1000,
    retry: retrySkipping403,
  });
}
