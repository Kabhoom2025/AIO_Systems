import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import type { ResourceUtilizationParams } from "../types";
import { retrySkipping403 } from "../utils/apiErrors";

export function useResourceUtilization(params: ResourceUtilizationParams | undefined) {
  return useQuery({
    queryKey: ["reports", "resource-utilization", params],
    queryFn: () => reportsApi.getResourceUtilization(params as ResourceUtilizationParams),
    enabled: !!params?.organizationId,
    staleTime: 30 * 1000,
    retry: retrySkipping403,
  });
}
