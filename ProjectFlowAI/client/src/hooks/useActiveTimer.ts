import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { timersApi } from "../api/timers";
import type { StartTimerRequest } from "../types";

export const ACTIVE_TIMER_KEY = ["timers", "active"] as const;

export function useActiveTimer() {
  return useQuery({
    queryKey: ACTIVE_TIMER_KEY,
    queryFn: () => timersApi.getActive(),
    // Keep the topbar timer widget fresh across the whole app without a per-second refetch —
    // the elapsed-time display itself is a local setInterval tick in the widget.
    refetchInterval: 30 * 1000,
    staleTime: 15 * 1000,
  });
}

export function useStartTimer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: StartTimerRequest) => timersApi.start(payload),
    onSuccess: (timer) => {
      queryClient.setQueryData(ACTIVE_TIMER_KEY, timer);
    },
    onError: (error: unknown) => {
      // 409 means a timer is already running for this user — refetch so the widget shows the
      // currently-running timer instead of just surfacing a raw error.
      const status = (error as { response?: { status?: number } })?.response?.status;
      if (status === 409) {
        queryClient.invalidateQueries({ queryKey: ACTIVE_TIMER_KEY });
      }
    },
  });
}

export function useStopTimer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => timersApi.stop(),
    onSuccess: () => {
      queryClient.setQueryData(ACTIVE_TIMER_KEY, null);
      // We don't know which work item the stopped timer belonged to from the response alone
      // (WorkItemTimeLog carries no workItemId), so invalidate broadly rather than guess.
      queryClient.invalidateQueries({ queryKey: ["workItems"] });
      queryClient.invalidateQueries({ queryKey: ["timesheet"] });
    },
  });
}
