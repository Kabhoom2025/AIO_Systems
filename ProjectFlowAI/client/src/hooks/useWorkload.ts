import { useQuery } from "@tanstack/react-query";
import { calendarApi } from "../api/calendar";
import type { CalendarQueryParams } from "../types";

export function useWorkload(params: CalendarQueryParams) {
  return useQuery({
    queryKey: ["workload", params],
    queryFn: () => calendarApi.getWorkload(params),
    enabled: !!params.organizationId,
    placeholderData: (prev) => prev,
    staleTime: 30 * 1000,
  });
}
