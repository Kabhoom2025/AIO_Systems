import { useQuery } from "@tanstack/react-query";
import { sprintsApi } from "../api/sprints";

export function useVelocity(projectId: string | undefined) {
  return useQuery({
    queryKey: ["velocity", projectId],
    queryFn: () => sprintsApi.getVelocity(projectId as string),
    enabled: !!projectId,
    staleTime: 30 * 1000,
  });
}
