import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import { retrySkipping403 } from "../utils/apiErrors";

export function useSprintDashboard(sprintId: string | undefined) {
  return useQuery({
    queryKey: ["reports", "sprint", sprintId],
    queryFn: () => reportsApi.getSprint(sprintId as string),
    enabled: !!sprintId,
    staleTime: 15 * 1000,
    retry: retrySkipping403,
  });
}
