import { useQuery } from "@tanstack/react-query";
import { auditLogsApi } from "../api/auditLogs";
import type { AuditLogListParams } from "../types";

export function useAuditLogs(params: AuditLogListParams) {
  return useQuery({
    queryKey: ["auditLogs", params],
    queryFn: () => auditLogsApi.list(params),
    enabled: !!params.organizationId,
    placeholderData: (prev) => prev,
    staleTime: 30 * 1000,
  });
}
