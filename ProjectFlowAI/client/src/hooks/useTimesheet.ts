import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { timesheetsApi } from "../api/timers";
import { workItemsApi } from "../api/workItems";
import type { TimesheetQueryParams } from "../types";

export function timesheetKey(params: TimesheetQueryParams) {
  return ["timesheet", params] as const;
}

export function useTimesheet(params: TimesheetQueryParams) {
  return useQuery({
    queryKey: timesheetKey(params),
    queryFn: () => timesheetsApi.get(params),
    enabled: !!params.userId,
    placeholderData: (prev) => prev,
    staleTime: 15 * 1000,
  });
}

export function useSetTimeLogBillable() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ timeLogId, isBillable }: { timeLogId: string; isBillable: boolean }) =>
      workItemsApi.setTimeLogBillable(timeLogId, isBillable),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["timesheet"] });
      queryClient.invalidateQueries({ queryKey: ["workItems", "detail"] });
    },
  });
}
