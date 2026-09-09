import { useQuery } from "@tanstack/react-query";
import { calendarApi } from "../api/calendar";
import type { CalendarQueryParams } from "../types";

export function useCalendarEvents(params: CalendarQueryParams) {
  return useQuery({
    queryKey: ["calendarEvents", params],
    queryFn: () => calendarApi.list(params),
    enabled: !!params.organizationId,
    placeholderData: (prev) => prev,
    staleTime: 30 * 1000,
  });
}
