import { axiosClient } from "./axiosClient";
import type { AuditLog, AuditLogListParams, PagedResult } from "../types";

export const auditLogsApi = {
  list: (params: AuditLogListParams) =>
    axiosClient
      .get<PagedResult<AuditLog>>("/audit-logs", { params })
      .then((r) => r.data),
};
