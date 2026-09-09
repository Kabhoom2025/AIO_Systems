import { axiosClient } from "./axiosClient";
import type {
  ActiveTimer,
  StartTimerRequest,
  TimesheetQueryParams,
  TimesheetResponse,
  WorkItemTimeLog,
} from "../types";

export const timersApi = {
  start: (payload: StartTimerRequest) =>
    axiosClient.post<ActiveTimer>("/timers/start", payload).then((r) => r.data),

  stop: () => axiosClient.post<WorkItemTimeLog>("/timers/stop").then((r) => r.data),

  // The backend may return 204 (or occasionally 404) when there is no running timer — treat
  // both defensively as "no active timer" rather than surfacing an error.
  getActive: async (): Promise<ActiveTimer | null> => {
    try {
      const response = await axiosClient.get<ActiveTimer | "">("/timers/active");
      if (!response.data || response.status === 204) return null;
      return response.data as ActiveTimer;
    } catch (error: unknown) {
      const status = (error as { response?: { status?: number } })?.response?.status;
      if (status === 204 || status === 404) return null;
      throw error;
    }
  },
};

export const timesheetsApi = {
  get: (params: TimesheetQueryParams) =>
    axiosClient.get<TimesheetResponse>("/timesheets", { params }).then((r) => r.data),
};
