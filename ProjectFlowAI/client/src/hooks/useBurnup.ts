import { useQuery } from "@tanstack/react-query";
import { sprintsApi } from "../api/sprints";

export function useBurnup(sprintId: string | undefined) {
  return useQuery({
    queryKey: ["burnup", sprintId],
    queryFn: () => sprintsApi.getBurnup(sprintId as string),
    enabled: !!sprintId,
    staleTime: 30 * 1000,
  });
}
