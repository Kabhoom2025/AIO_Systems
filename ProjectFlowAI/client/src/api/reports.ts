import { axiosClient } from "./axiosClient";
import { toKanbanCard, type RawKanbanWorkItem } from "./workItems";
import type {
  CostAnalysisParams,
  CostAnalysisReport,
  CycleTimeParams,
  CycleTimeReport,
  ExecutiveDashboardParams,
  ExecutiveDashboardReport,
  ProductivityParams,
  ProductivityReport,
  ProjectHealthReport,
  ResourceUtilizationParams,
  ResourceUtilizationReport,
  SprintReport,
} from "../types";

interface RawSprintReport extends Omit<SprintReport, "blockedItems"> {
  blockedItems: RawKanbanWorkItem[];
}

export const reportsApi = {
  getExecutive: (params: ExecutiveDashboardParams) =>
    axiosClient
      .get<ExecutiveDashboardReport>("/reports/executive", { params })
      .then((r) => r.data),

  getSprint: (sprintId: string) =>
    axiosClient.get<RawSprintReport>(`/reports/sprint/${sprintId}`).then((r) => ({
      ...r.data,
      blockedItems: r.data.blockedItems.map(toKanbanCard),
    })),

  getCycleTime: (params: CycleTimeParams) =>
    axiosClient.get<CycleTimeReport>("/reports/cycle-time", { params }).then((r) => r.data),

  getProjectHealth: (projectId: string) =>
    axiosClient
      .get<ProjectHealthReport>("/reports/project-health", { params: { projectId } })
      .then((r) => r.data),

  getProductivity: (params: ProductivityParams) =>
    axiosClient.get<ProductivityReport>("/reports/productivity", { params }).then((r) => r.data),

  getResourceUtilization: (params: ResourceUtilizationParams) =>
    axiosClient
      .get<ResourceUtilizationReport>("/reports/resource-utilization", { params })
      .then((r) => r.data),

  getCostAnalysis: (params: CostAnalysisParams) =>
    axiosClient.get<CostAnalysisReport>("/reports/cost-analysis", { params }).then((r) => r.data),
};
