import { useQuery } from "@tanstack/react-query";
import { sprintsApi } from "../api/sprints";

export function useBurndown(sprintId: string | undefined) {
  return useQuery({
    queryKey: ["burndown", sprintId],
    queryFn: () => sprintsApi.getBurndown(sprintId as string),
    enabled: !!sprintId,
    staleTime: 30 * 1000,
  });
}
