import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import type { CostAnalysisParams } from "../types";
import { retrySkipping403 } from "../utils/apiErrors";

export function useCostAnalysis(params: CostAnalysisParams | undefined) {
  return useQuery({
    queryKey: ["reports", "cost-analysis", params],
    queryFn: () => reportsApi.getCostAnalysis(params as CostAnalysisParams),
    enabled: !!params?.projectId,
    staleTime: 30 * 1000,
    retry: retrySkipping403,
  });
}
