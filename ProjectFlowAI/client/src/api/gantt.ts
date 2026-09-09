import { axiosClient } from "./axiosClient";
import type {
  CreateBaselineRequest,
  CriticalPath,
  GanttBaseline,
  GanttBaselineDetail,
  GanttChart,
} from "../types";

export const ganttApi = {
  get: (projectId: string) =>
    axiosClient.get<GanttChart>(`/projects/${projectId}/gantt`).then((r) => r.data),

  getCriticalPath: (projectId: string) =>
    axiosClient.get<CriticalPath>(`/projects/${projectId}/gantt/critical-path`).then((r) => r.data),

  createBaseline: (projectId: string, payload: CreateBaselineRequest) =>
    axiosClient
      .post<GanttBaseline>(`/projects/${projectId}/gantt/baselines`, payload)
      .then((r) => r.data),

  listBaselines: (projectId: string) =>
    axiosClient
      .get<GanttBaseline[]>(`/projects/${projectId}/gantt/baselines`)
      .then((r) => r.data),

  getBaseline: (projectId: string, baselineId: string) =>
    axiosClient
      .get<GanttBaselineDetail>(`/projects/${projectId}/gantt/baselines/${baselineId}`)
      .then((r) => r.data),

  // Blob download — the browser must save a real .csv file, not open it inline.
  exportCsv: async (projectId: string, projectKey: string) => {
    const response = await axiosClient.get(`/projects/${projectId}/gantt/export`, {
      params: { format: "csv" },
      responseType: "blob",
    });
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement("a");
    link.href = url;
    link.download = `${projectKey || "project"}-gantt.csv`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },
};
