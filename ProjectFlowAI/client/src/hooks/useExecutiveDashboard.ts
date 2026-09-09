import { useQuery } from "@tanstack/react-query";
import { reportsApi } from "../api/reports";
import { retrySkipping403 } from "../utils/apiErrors";

export function useExecutiveDashboard(organizationId: string | undefined) {
  return useQuery({
    queryKey: ["reports", "executive", organizationId],
    queryFn: () => reportsApi.getExecutive({ organizationId: organizationId as string }),
    enabled: !!organizationId,
    staleTime: 30 * 1000,
    retry: retrySkipping403,
  });
}
