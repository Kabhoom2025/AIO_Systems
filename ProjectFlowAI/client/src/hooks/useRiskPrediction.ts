import { useQuery } from "@tanstack/react-query";
import { aiApi } from "../api/ai";
import { retrySkipping503 } from "../utils/apiErrors";

export function useRiskPrediction(projectId: string | undefined) {
  return useQuery({
    queryKey: ["ai", "riskPrediction", projectId],
    queryFn: () => aiApi.getRiskPrediction(projectId as string),
    enabled: !!projectId,
    retry: retrySkipping503,
    staleTime: 60 * 1000,
  });
}
