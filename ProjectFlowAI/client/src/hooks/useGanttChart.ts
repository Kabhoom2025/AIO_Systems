import { useQuery } from "@tanstack/react-query";
import { ganttApi } from "../api/gantt";

export function useGanttChart(projectId: string | undefined) {
  return useQuery({
    queryKey: ["gantt", projectId],
    queryFn: () => ganttApi.get(projectId as string),
    enabled: !!projectId,
    staleTime: 15 * 1000,
  });
}
