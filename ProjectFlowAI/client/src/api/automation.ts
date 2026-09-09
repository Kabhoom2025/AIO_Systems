import { axiosClient } from "./axiosClient";
import type {
  Approval,
  CreateWorkflowRequest,
  DecideApprovalRequest,
  PagedResult,
  UpdateWorkflowRequest,
  WorkflowDetail,
  WorkflowRun,
  WorkflowSummary,
} from "../types";

export const automationApi = {
  listWorkflows: (projectId: string) =>
    axiosClient
      .get<WorkflowSummary[]>("/automation/workflows", { params: { projectId } })
      .then((r) => r.data),

  createWorkflow: (payload: CreateWorkflowRequest) =>
    axiosClient.post<WorkflowDetail>("/automation/workflows", payload).then((r) => r.data),

  getWorkflow: (id: string) =>
    axiosClient.get<WorkflowDetail>(`/automation/workflows/${id}`).then((r) => r.data),

  updateWorkflow: (id: string, payload: UpdateWorkflowRequest) =>
    axiosClient.put<WorkflowDetail>(`/automation/workflows/${id}`, payload).then((r) => r.data),

  deleteWorkflow: (id: string) =>
    axiosClient.delete<void>(`/automation/workflows/${id}`).then((r) => r.data),

  enableWorkflow: (id: string) =>
    axiosClient.post<void>(`/automation/workflows/${id}/enable`).then((r) => r.data),

  disableWorkflow: (id: string) =>
    axiosClient.post<void>(`/automation/workflows/${id}/disable`).then((r) => r.data),

  getRuns: (id: string, page: number, pageSize: number) =>
    axiosClient
      .get<PagedResult<WorkflowRun>>(`/automation/workflows/${id}/runs`, {
        params: { page, pageSize },
      })
      .then((r) => r.data),

  listApprovals: (assignedToMe: boolean) =>
    axiosClient
      .get<Approval[]>("/automation/approvals", { params: { assignedToMe } })
      .then((r) => r.data),

  decideApproval: (id: string, payload: DecideApprovalRequest) =>
    axiosClient.post<void>(`/automation/approvals/${id}/decide`, payload).then((r) => r.data),
};
