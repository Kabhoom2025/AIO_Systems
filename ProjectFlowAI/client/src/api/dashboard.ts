import { axiosClient } from "./axiosClient";
import { toKanbanCard, type RawKanbanWorkItem } from "./workItems";
import type { DashboardResponse, DashboardTaskCard } from "../types";

interface RawDashboardResponse extends Omit<DashboardResponse, "todaysTasks" | "overdueTasks"> {
  todaysTasks: RawKanbanWorkItem[];
  overdueTasks: RawKanbanWorkItem[];
}

function toDashboardTaskCard(w: RawKanbanWorkItem): DashboardTaskCard {
  return { ...toKanbanCard(w), projectId: w.projectId };
}

function toDashboardResponse(dto: RawDashboardResponse): DashboardResponse {
  return {
    ...dto,
    todaysTasks: dto.todaysTasks.map(toDashboardTaskCard),
    overdueTasks: dto.overdueTasks.map(toDashboardTaskCard),
  };
}

export const dashboardApi = {
  get: () =>
    axiosClient.get<RawDashboardResponse>("/dashboard").then((r) => toDashboardResponse(r.data)),
};
