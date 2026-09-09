import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import type { CycleTimeParams } from "../types";
import { retrySkipping403 } from "../utils/apiErrors";

export function useCycleTimeReport(params: CycleTimeParams | undefined) {
  return useQuery({
    queryKey: ["reports", "cycle-time", params],
    queryFn: () => reportsApi.getCycleTime(params as CycleTimeParams),
    enabled: !!params?.projectId,
    staleTime: 30 * 1000,
    retry: retrySkipping403,
  });
}
