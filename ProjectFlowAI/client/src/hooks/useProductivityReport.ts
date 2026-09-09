import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import type { ProductivityParams } from "../types";
import { retrySkipping403 } from "../utils/apiErrors";

export function useProductivityReport(params: ProductivityParams | undefined) {
  return useQuery({
    queryKey: ["reports", "productivity", params],
    queryFn: () => reportsApi.getProductivity(params as ProductivityParams),
    enabled: !!params?.projectId,
    staleTime: 30 * 1000,
    retry: retrySkipping403,
  });
}
