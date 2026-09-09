import { useMutation, useQueryClient } from "@tanstack/react-query";
import { workItemsApi } from "../api/workItems";
import type { CreateTimeLogRequest } from "../types";

export function useTimeLogs(workItemId: string) {
  const queryClient = useQueryClient();
  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["workItems", "detail", workItemId] });

  const addTimeLog = useMutation({
    mutationFn: (payload: CreateTimeLogRequest) => workItemsApi.addTimeLog(workItemId, payload),
    onSuccess: invalidate,
  });

  const removeTimeLog = useMutation({
    mutationFn: (timeLogId: string) => workItemsApi.removeTimeLog(timeLogId),
    onSuccess: invalidate,
  });

  return { addTimeLog, removeTimeLog };
}
