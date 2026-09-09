import { createBrowserRouter, Navigate } from "react-router-dom";
import { ForgotPasswordPage } from "../features/auth/ForgotPasswordPage";
import { LoginPage } from "../features/auth/LoginPage";
import { RegisterPage } from "../features/auth/RegisterPage";
import { ResetPasswordPage } from "../features/auth/ResetPasswordPage";
import { TwoFactorSetupPage } from "../features/auth/TwoFactorSetupPage";
import { TwoFactorVerifyPage } from "../features/auth/TwoFactorVerifyPage";
import { VerifyEmailPage } from "../features/auth/VerifyEmailPage";
import { AuditLogsPage } from "../features/organization/AuditLogsPage";
import { DashboardPage } from "../features/dashboard/DashboardPage";
import { DepartmentsPage } from "../features/organization/DepartmentsPage";
import { InvitationsPage } from "../features/organization/InvitationsPage";
import { OrganizationSettingsPage } from "../features/organization/OrganizationSettingsPage";
import { RolesPermissionsPage } from "../features/organization/RolesPermissionsPage";
import { TeamsPage } from "../features/organization/TeamsPage";
import { UsersPage } from "../features/organization/UsersPage";
import { ProjectsListPage } from "../features/projects/ProjectsListPage";
import { ProjectWorkspaceLayout } from "../features/projects/ProjectWorkspaceLayout";
import { KanbanBoardPage } from "../features/projects/KanbanBoardPage";
import { WorkItemListPage } from "../features/projects/WorkItemListPage";
import { MilestonesPage } from "../features/projects/MilestonesPage";
import { ProjectSettingsPage } from "../features/projects/ProjectSettingsPage";
import { SprintsPage } from "../features/sprints/SprintsPage";
import { SprintBoardPage } from "../features/sprints/SprintBoardPage";
import { RetrospectivePage } from "../features/sprints/RetrospectivePage";
import { GanttChartPage } from "../features/gantt/GanttChartPage";
import { CalendarPage } from "../features/calendar/CalendarPage";
import { TimesheetsPage } from "../features/timesheets/TimesheetsPage";
import { ChatPage } from "../features/chat/ChatPage";
import { WikiPage } from "../features/docs/WikiPage";
import { ProjectDocumentsPage } from "../features/projects/ProjectDocumentsPage";
import { NotificationPreferencesPage } from "../features/notifications/NotificationPreferencesPage";
import { NotificationsListPage } from "../features/notifications/NotificationsListPage";
import { IntegrationSettingsPage } from "../features/notifications/IntegrationSettingsPage";
import { ReportsHomePage } from "../features/reports/ReportsHomePage";
import { ExecutiveDashboardPage } from "../features/reports/ExecutiveDashboardPage";
import { SprintDashboardPage } from "../features/reports/SprintDashboardPage";
import { CycleTimeReportPage } from "../features/reports/CycleTimeReportPage";
import { ProjectHealthPage } from "../features/reports/ProjectHealthPage";
import { ProductivityReportPage } from "../features/reports/ProductivityReportPage";
import { ResourceUtilizationPage } from "../features/reports/ResourceUtilizationPage";
import { CostAnalysisPage } from "../features/reports/CostAnalysisPage";
import { ProjectReportsTab } from "../features/projects/ProjectReportsTab";
import { AiToolsHubPage } from "../features/ai/AiToolsHubPage";
import { TaskGeneratorPage } from "../features/ai/TaskGeneratorPage";
import { SprintPlannerPage } from "../features/ai/SprintPlannerPage";
import { MeetingSummarizerPage } from "../features/ai/MeetingSummarizerPage";
import { AiChatPage } from "../features/ai/AiChatPage";
import { ResourceAllocationPage } from "../features/ai/ResourceAllocationPage";
import { TaskPrioritizerPage } from "../features/ai/TaskPrioritizerPage";
import { CodeReviewPage } from "../features/ai/CodeReviewPage";
import { ReleaseNotesGeneratorPage } from "../features/ai/ReleaseNotesGeneratorPage";
import { WorkflowListPage } from "../features/automation/WorkflowListPage";
import { WorkflowEditorPage } from "../features/automation/WorkflowEditorPage";
import { WorkflowRunsPage } from "../features/automation/WorkflowRunsPage";
import { ApprovalsInboxPage } from "../features/automation/ApprovalsInboxPage";
import { ProtectedRoute } from "./ProtectedRoute";

export const router = createBrowserRouter([
  { path: "/login", element: <LoginPage /> },
  { path: "/register", element: <RegisterPage /> },
  { path: "/forgot-password", element: <ForgotPasswordPage /> },
  { path: "/reset-password", element: <ResetPasswordPage /> },
  { path: "/verify-email", element: <VerifyEmailPage /> },
  { path: "/2fa/verify", element: <TwoFactorVerifyPage /> },
  {
    path: "/2fa/setup",
    element: (
      <ProtectedRoute>
        <TwoFactorSetupPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <DashboardPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/organization",
    element: (
      <ProtectedRoute>
        <OrganizationSettingsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/departments",
    element: (
      <ProtectedRoute>
        <DepartmentsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/teams",
    element: (
      <ProtectedRoute>
        <TeamsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/users",
    element: (
      <ProtectedRoute>
        <UsersPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/invitations",
    element: (
      <ProtectedRoute>
        <InvitationsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/roles",
    element: (
      <ProtectedRoute>
        <RolesPermissionsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/audit-logs",
    element: (
      <ProtectedRoute>
        <AuditLogsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/projects",
    element: (
      <ProtectedRoute>
        <ProjectsListPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/calendar",
    element: (
      <ProtectedRoute>
        <CalendarPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/my-time",
    element: (
      <ProtectedRoute>
        <TimesheetsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/chat",
    element: (
      <ProtectedRoute>
        <ChatPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/wiki",
    element: (
      <ProtectedRoute>
        <WikiPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/notifications",
    element: (
      <ProtectedRoute>
        <NotificationsListPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/notification-preferences",
    element: (
      <ProtectedRoute>
        <NotificationPreferencesPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/integration-settings",
    element: (
      <ProtectedRoute>
        <IntegrationSettingsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports",
    element: (
      <ProtectedRoute>
        <ReportsHomePage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/executive",
    element: (
      <ProtectedRoute>
        <ExecutiveDashboardPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/sprint",
    element: (
      <ProtectedRoute>
        <SprintDashboardPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/cycle-time",
    element: (
      <ProtectedRoute>
        <CycleTimeReportPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/project-health",
    element: (
      <ProtectedRoute>
        <ProjectHealthPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/productivity",
    element: (
      <ProtectedRoute>
        <ProductivityReportPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/resource-utilization",
    element: (
      <ProtectedRoute>
        <ResourceUtilizationPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/reports/cost-analysis",
    element: (
      <ProtectedRoute>
        <CostAnalysisPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/approvals",
    element: (
      <ProtectedRoute>
        <ApprovalsInboxPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/projects/:projectId",
    element: (
      <ProtectedRoute>
        <ProjectWorkspaceLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="board" replace /> },
      { path: "board", element: <KanbanBoardPage /> },
      { path: "list", element: <WorkItemListPage /> },
      { path: "sprints", element: <SprintsPage /> },
      { path: "sprints/:sprintId/board", element: <SprintBoardPage /> },
      { path: "sprints/:sprintId/retrospective", element: <RetrospectivePage /> },
      { path: "gantt", element: <GanttChartPage /> },
      { path: "calendar", element: <CalendarPage /> },
      { path: "milestones", element: <MilestonesPage /> },
      { path: "documents", element: <ProjectDocumentsPage /> },
      { path: "reports", element: <ProjectReportsTab /> },
      { path: "ai", element: <AiToolsHubPage /> },
      { path: "ai/generate-tasks", element: <TaskGeneratorPage /> },
      { path: "ai/sprint-planner", element: <SprintPlannerPage /> },
      { path: "ai/meeting-summarizer", element: <MeetingSummarizerPage /> },
      { path: "ai/chat", element: <AiChatPage /> },
      { path: "ai/resource-allocation", element: <ResourceAllocationPage /> },
      { path: "ai/prioritize", element: <TaskPrioritizerPage /> },
      { path: "ai/code-review", element: <CodeReviewPage /> },
      { path: "ai/release-notes", element: <ReleaseNotesGeneratorPage /> },
      { path: "automation", element: <WorkflowListPage /> },
      { path: "automation/new", element: <WorkflowEditorPage /> },
      { path: "automation/:workflowId/edit", element: <WorkflowEditorPage /> },
      { path: "automation/:workflowId/runs", element: <WorkflowRunsPage /> },
      { path: "settings", element: <ProjectSettingsPage /> },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
