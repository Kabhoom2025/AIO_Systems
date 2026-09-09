import { axiosClient } from "./axiosClient";
import type { CalendarEventsResponse, CalendarQueryParams, CalendarWorkloadResponse } from "../types";

export const calendarApi = {
  list: (params: CalendarQueryParams) =>
    axiosClient.get<CalendarEventsResponse>("/calendar", { params }).then((r) => r.data),

  getWorkload: (params: CalendarQueryParams) =>
    axiosClient.get<CalendarWorkloadResponse>("/calendar/workload", { params }).then((r) => r.data),
};
