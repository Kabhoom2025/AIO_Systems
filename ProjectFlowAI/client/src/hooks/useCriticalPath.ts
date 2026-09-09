import { useQuery } from "@tanstack/react-query";
import { ganttApi } from "../api/gantt";

export function useCriticalPath(projectId: string | undefined, enabled: boolean) {
  return useQuery({
    queryKey: ["criticalPath", projectId],
    queryFn: () => ganttApi.getCriticalPath(projectId as string),
    enabled: !!projectId && enabled,
    staleTime: 30 * 1000,
  });
}
