import { useQuery } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import { retrySkipping503 } from "../utils/apiErrors";

export function useResourceAllocationAi(projectId: string | undefined) {
  return useQuery({
    queryKey: ["ai", "resourceAllocation", projectId],
    queryFn: () => aiApi.getResourceAllocation(projectId as string),
    enabled: !!projectId,
    retry: retrySkipping503,
    staleTime: 60 * 1000,
  });
}
