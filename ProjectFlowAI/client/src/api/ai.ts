import { axiosClient } from "./axiosClient";
import type {
  AiChatMessage,
  AiChatRequest,
  AiChatResponse,
  AiConversationSummary,
  AnalyzeBugRequest,
  AnalyzeBugResponse,
  DeadlinePredictionResponse,
  EstimateStoryPointsRequest,
  EstimateStoryPointsResponse,
  GenerateReleaseNotesRequest,
  GenerateReleaseNotesResponse,
  GenerateTasksRequest,
  GenerateTasksResponse,
  PlanSprintRequest,
  PlanSprintResponse,
  PrioritizeTasksRequest,
  PrioritizeTasksResponse,
  ResourceAllocationResponse,
  ReviewCodeRequest,
  ReviewCodeResponse,
  RiskPredictionResponse,
  SummarizeMeetingRequest,
  SummarizeMeetingResponse,
} from "../types";

export const aiApi = {
  generateTasks: (payload: GenerateTasksRequest) =>
    axiosClient.post<GenerateTasksResponse>("/ai/generate-tasks", payload).then((r) => r.data),

  planSprint: (payload: PlanSprintRequest) =>
    axiosClient.post<PlanSprintResponse>("/ai/plan-sprint", payload).then((r) => r.data),

  analyzeBug: (payload: AnalyzeBugRequest) =>
    axiosClient.post<AnalyzeBugResponse>("/ai/analyze-bug", payload).then((r) => r.data),

  summarizeMeeting: (payload: SummarizeMeetingRequest) =>
    axiosClient.post<SummarizeMeetingResponse>("/ai/summarize-meeting", payload).then((r) => r.data),

  chat: (payload: AiChatRequest) =>
    axiosClient.post<AiChatResponse>("/ai/chat", payload).then((r) => r.data),

  listConversations: (projectId: string) =>
    axiosClient
      .get<AiConversationSummary[]>("/ai/chat/conversations", { params: { projectId } })
      .then((r) => r.data),

  getConversationMessages: (conversationId: string) =>
    axiosClient
      .get<AiChatMessage[]>(`/ai/chat/conversations/${conversationId}/messages`)
      .then((r) => r.data),

  getRiskPrediction: (projectId: string) =>
    axiosClient
      .get<RiskPredictionResponse>("/ai/risk-prediction", { params: { projectId } })
      .then((r) => r.data),

  getResourceAllocation: (projectId: string) =>
    axiosClient
      .get<ResourceAllocationResponse>("/ai/resource-allocation", { params: { projectId } })
      .then((r) => r.data),

  estimateStoryPoints: (payload: EstimateStoryPointsRequest) =>
    axiosClient
      .post<EstimateStoryPointsResponse>("/ai/estimate-story-points", payload)
      .then((r) => r.data),

  getPredictDeadline: (projectId: string) =>
    axiosClient
      .get<DeadlinePredictionResponse>("/ai/predict-deadline", { params: { projectId } })
      .then((r) => r.data),

  prioritizeTasks: (payload: PrioritizeTasksRequest) =>
    axiosClient.post<PrioritizeTasksResponse>("/ai/prioritize-tasks", payload).then((r) => r.data),

  reviewCode: (payload: ReviewCodeRequest) =>
    axiosClient.post<ReviewCodeResponse>("/ai/review-code", payload).then((r) => r.data),

  generateReleaseNotes: (payload: GenerateReleaseNotesRequest) =>
    axiosClient
      .post<GenerateReleaseNotesResponse>("/ai/generate-release-notes", payload)
      .then((r) => r.data),
};
