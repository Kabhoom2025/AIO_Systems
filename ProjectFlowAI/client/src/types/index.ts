// Shared TypeScript types matching the ProjectFlow AI backend API contract.
// Base API URL: http://localhost:5000/api/projectflow

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

// ---------- Auth ----------

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string | null;
  organizationId: string;
  roles: string[];
  permissions: string[];
  isEmailVerified: boolean;
  twoFactorEnabled: boolean;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface AuthResponse extends AuthTokens {
  user: User;
}

export interface RequiresTwoFactorResponse {
  requiresTwoFactor: true;
  tempToken: string;
}

export type LoginResponse = AuthResponse | RequiresTwoFactorResponse;

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface TwoFactorVerifyRequest {
  tempToken: string;
  code: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface RefreshResponse extends AuthTokens {}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface VerifyEmailRequest {
  token: string;
}

export interface TwoFactorEnableResponse {
  secret: string;
  qrCodeUri: string;
}

export interface GoogleOAuthRequest {
  idToken: string;
}

export interface MicrosoftOAuthRequest {
  accessToken: string;
}

// ---------- Organizations ----------

export interface Organization {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string | null;
  domain?: string | null;
  subscriptionPlan: string;
  isActive: boolean;
}

export interface CreateOrganizationRequest {
  name: string;
  slug: string;
  domain?: string;
}

export type UpdateOrganizationRequest = Partial<CreateOrganizationRequest> & {
  logoUrl?: string;
  isActive?: boolean;
};

// ---------- Departments ----------

export interface Department {
  id: string;
  organizationId: string;
  name: string;
  description?: string | null;
  parentDepartmentId?: string | null;
}

export interface CreateDepartmentRequest {
  organizationId: string;
  name: string;
  description?: string;
  parentDepartmentId?: string | null;
}

export type UpdateDepartmentRequest = Partial<Omit<CreateDepartmentRequest, "organizationId">>;

// ---------- Teams ----------

export interface TeamMember {
  userId: string;
  roleInTeam: string;
}

export interface Team {
  id: string;
  organizationId: string;
  departmentId?: string | null;
  name: string;
  description?: string | null;
  members?: TeamMember[];
}

export interface CreateTeamRequest {
  organizationId: string;
  departmentId?: string | null;
  name: string;
  description?: string;
}

export type UpdateTeamRequest = Partial<Omit<CreateTeamRequest, "organizationId">>;

export interface AddTeamMemberRequest {
  userId: string;
  roleInTeam: string;
}

// ---------- Users ----------

export interface UserListParams {
  organizationId: string;
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
}

// ---------- Invitations ----------

export interface Invitation {
  id: string;
  organizationId: string;
  email: string;
  roleId: string;
  status?: string;
  createdAt?: string;
  expiresAt?: string;
}

export interface CreateInvitationRequest {
  organizationId: string;
  email: string;
  roleId: string;
}

export interface AcceptInvitationRequest {
  token: string;
}

// ---------- Roles & Permissions ----------

export interface Role {
  id: string;
  organizationId: string;
  name: string;
  // Matches the backend's RoleDto.Permissions field exactly — do not rename to
  // permissionKeys, which is only the *request* shape for creating a role.
  permissions: string[];
  isSystemRole?: boolean;
}

export interface CreateRoleRequest {
  organizationId: string;
  name: string;
  permissionKeys: string[];
}

export interface Permission {
  key: string;
  description: string;
  category: string;
}

// ---------- Audit Logs ----------

export interface AuditLog {
  id: string;
  organizationId: string;
  userId: string;
  action: string;
  entityType: string;
  entityId: string;
  createdAt: string;
}

export interface AuditLogListParams {
  organizationId: string;
  entityType?: string;
  userId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

// ---------- Projects ----------

export type ProjectStatus = "Active" | "OnHold" | "Completed" | "Archived";

export interface Project {
  id: string;
  organizationId: string;
  key: string;
  name: string;
  description?: string | null;
  status: ProjectStatus;
  startDate?: string | null;
  endDate?: string | null;
  ownerUserId?: string | null;
  isArchived: boolean;
  createdAt: string;
}

export interface ProjectDetail extends Project {
  workItemCountsByStatus: Record<string, number>;
  memberCount: number;
}

export interface ProjectListParams {
  organizationId: string;
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateProjectRequest {
  organizationId: string;
  key: string;
  name: string;
  description?: string;
  startDate?: string | null;
  endDate?: string | null;
  ownerUserId?: string | null;
}

export type UpdateProjectRequest = Partial<
  Omit<CreateProjectRequest, "organizationId">
> & {
  status?: ProjectStatus;
};

export interface ProjectMember {
  userId: string;
  roleInProject: string;
}

export interface AddProjectMemberRequest {
  userId: string;
  roleInProject: string;
}

// ---------- Milestones ----------

export type MilestoneStatus = "Open" | "Completed";

export interface Milestone {
  id: string;
  projectId: string;
  name: string;
  description?: string | null;
  dueDate?: string | null;
  status: MilestoneStatus;
}

export interface CreateMilestoneRequest {
  projectId: string;
  name: string;
  description?: string;
  dueDate?: string | null;
}

export type UpdateMilestoneRequest = Partial<Omit<CreateMilestoneRequest, "projectId">> & {
  status?: MilestoneStatus;
};

// ---------- Labels ----------

export interface Label {
  id: string;
  projectId: string;
  name: string;
  colorHex: string;
}

export interface CreateLabelRequest {
  projectId: string;
  name: string;
  colorHex: string;
}

// ---------- Work Items (UI concept: "Task") ----------

export type WorkItemStatus =
  | "Backlog"
  | "ToDo"
  | "InProgress"
  | "CodeReview"
  | "Testing"
  | "Blocked"
  | "Done";

export type WorkItemPriority = "Lowest" | "Low" | "Medium" | "High" | "Highest";

export type WorkItemType = "Task" | "Bug" | "Story" | "Epic";

export const WORK_ITEM_STATUSES: WorkItemStatus[] = [
  "Backlog",
  "ToDo",
  "InProgress",
  "CodeReview",
  "Testing",
  "Blocked",
  "Done",
];

export const WORK_ITEM_STATUS_LABELS: Record<WorkItemStatus, string> = {
  Backlog: "Backlog",
  ToDo: "To Do",
  InProgress: "In Progress",
  CodeReview: "Code Review",
  Testing: "Testing",
  Blocked: "Blocked",
  Done: "Done",
};

export const WORK_ITEM_PRIORITIES: WorkItemPriority[] = [
  "Lowest",
  "Low",
  "Medium",
  "High",
  "Highest",
];

export const WORK_ITEM_TYPES: WorkItemType[] = ["Task", "Bug", "Story", "Epic"];

export interface WorkItem {
  id: string;
  projectId: string;
  parentWorkItemId?: string | null;
  title: string;
  status: WorkItemStatus;
  priority: WorkItemPriority;
  type: WorkItemType;
  storyPoints?: number | null;
  estimatedHours?: number | null;
  assigneeUserId?: string | null;
  dueDate?: string | null;
  isRecurring: boolean;
  position: number;
  // Added in Phase 3 — set via POST /work-items/{id}/sprint; null/undefined means backlog.
  sprintId?: string | null;
}

export interface WorkItemListParams {
  projectId: string;
  status?: string;
  assigneeId?: string;
  priority?: string;
  labelId?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface KanbanCard {
  id: string;
  title: string;
  priority: WorkItemPriority;
  type: WorkItemType;
  storyPoints?: number | null;
  assigneeUserId?: string | null;
  assigneeName?: string | null;
  labelColors: string[];
  checklistDone: number;
  checklistTotal: number;
  commentCount: number;
  dueDate?: string | null;
  position: number;
}

export type KanbanBoard = Record<
  "backlog" | "todo" | "inProgress" | "codeReview" | "testing" | "blocked" | "done",
  KanbanCard[]
>;

export const KANBAN_COLUMN_TO_STATUS: Record<keyof KanbanBoard, WorkItemStatus> = {
  backlog: "Backlog",
  todo: "ToDo",
  inProgress: "InProgress",
  codeReview: "CodeReview",
  testing: "Testing",
  blocked: "Blocked",
  done: "Done",
};

export const STATUS_TO_KANBAN_COLUMN: Record<WorkItemStatus, keyof KanbanBoard> = {
  Backlog: "backlog",
  ToDo: "todo",
  InProgress: "inProgress",
  CodeReview: "codeReview",
  Testing: "testing",
  Blocked: "blocked",
  Done: "done",
};

export interface CreateWorkItemRequest {
  projectId: string;
  parentWorkItemId?: string | null;
  title: string;
  status?: WorkItemStatus;
  priority?: WorkItemPriority;
  type?: WorkItemType;
  storyPoints?: number | null;
  estimatedHours?: number | null;
  assigneeUserId?: string | null;
  dueDate?: string | null;
  isRecurring?: boolean;
}

export type UpdateWorkItemRequest = Partial<Omit<CreateWorkItemRequest, "projectId">> & {
  description?: string;
};

export interface MoveWorkItemRequest {
  status: WorkItemStatus;
  position: number;
}

export interface ChecklistItem {
  id: string;
  text: string;
  isDone: boolean;
  position: number;
}

export interface WorkItemLabel {
  id: string;
  name: string;
  colorHex: string;
}

export interface WorkItemFollower {
  userId: string;
  name: string;
}

export interface WorkItemDependency {
  id: string;
  dependsOnWorkItemId: string;
  dependsOnTitle: string;
  dependsOnStatus: WorkItemStatus;
  dependencyType: string;
}

export interface WorkItemComment {
  id: string;
  authorUserId: string;
  authorName: string;
  body: string;
  mentionedUserIds: string[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface WorkItemAttachment {
  id: string;
  fileName: string;
  fileSizeBytes: number;
  uploadedByUserId: string;
  createdAt: string;
  downloadUrl: string;
}

export interface WorkItemActivity {
  id: string;
  userId: string;
  userName: string;
  action: string;
  fieldName?: string | null;
  oldValue?: string | null;
  newValue?: string | null;
  createdAt: string;
}

export interface WorkItemTimeLog {
  id: string;
  userId: string;
  userName: string;
  minutes: number;
  note?: string | null;
  loggedDate: string;
  // Added in Phase 3 — toggled via PATCH /work-items/time-logs/{id}/billable.
  isBillable?: boolean;
}

export type CustomFieldType = "Text" | "Number" | "Date" | "Dropdown" | "Checkbox";

export interface CustomFieldValue {
  customFieldDefinitionId: string;
  fieldName: string;
  fieldType: CustomFieldType;
  valueJson: string;
}

export interface WorkItemDetail extends WorkItem {
  description?: string | null;
  checklistItems: ChecklistItem[];
  labels: WorkItemLabel[];
  followers: WorkItemFollower[];
  dependencies: WorkItemDependency[];
  comments: WorkItemComment[];
  attachments: WorkItemAttachment[];
  activity: WorkItemActivity[];
  timeLogs: WorkItemTimeLog[];
  actualHours: number;
  customFieldValues: CustomFieldValue[];
}

export interface CreateCommentRequest {
  body: string;
  mentionedUserIds: string[];
}

export interface UpdateCommentRequest {
  body: string;
  mentionedUserIds?: string[];
}

export interface CreateTimeLogRequest {
  minutes: number;
  note?: string;
  loggedDate: string;
}

export interface CreateDependencyRequest {
  dependsOnWorkItemId: string;
  dependencyType: string;
}

export interface CustomFieldDefinition {
  id: string;
  projectId: string;
  name: string;
  fieldType: CustomFieldType;
  optionsJson?: string | null;
  isRequired: boolean;
}

export interface CreateCustomFieldRequest {
  projectId: string;
  name: string;
  fieldType: CustomFieldType;
  optionsJson?: string;
  isRequired: boolean;
}

// ---------- Sprints ----------

export type SprintStatus = "Planned" | "Active" | "Completed";

export const SPRINT_STATUSES: SprintStatus[] = ["Planned", "Active", "Completed"];

export interface Sprint {
  id: string;
  projectId: string;
  name: string;
  goal?: string | null;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  workItemCount: number;
  completedWorkItemCount: number;
  totalStoryPoints: number;
  completedStoryPoints: number;
  createdAt: string;
}

export interface SprintListParams {
  projectId: string;
  status?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
}

export interface CreateSprintRequest {
  projectId: string;
  name: string;
  goal?: string;
  startDate: string;
  endDate: string;
}

export type UpdateSprintRequest = Partial<Omit<CreateSprintRequest, "projectId">>;

// Sprint board — a different container shape than the project Kanban board (an array of
// {status, items} columns rather than a keyed object), but items reuse the exact same
// KanbanCard DTO shape from Phase 2.
export interface SprintBoardColumn {
  status: WorkItemStatus;
  items: KanbanCard[];
}

export interface SprintBoard {
  sprintId: string;
  columns: SprintBoardColumn[];
}

export interface MoveWorkItemToSprintRequest {
  sprintId: string | null;
}

export interface VelocitySprintPoint {
  sprintId: string;
  sprintName: string;
  committedPoints: number;
  completedPoints: number;
}

export interface VelocityData {
  sprints: VelocitySprintPoint[];
}

export interface BurndownDayPoint {
  date: string;
  remainingPoints: number;
  idealRemainingPoints: number;
}

export interface BurndownData {
  days: BurndownDayPoint[];
}

export interface BurnupDayPoint {
  date: string;
  completedPoints: number;
  totalScopePoints: number;
}

export interface BurnupData {
  days: BurnupDayPoint[];
}

// ---------- Retrospectives ----------

export type RetroCategory = "WentWell" | "WentWrong" | "ActionItem";

export const RETRO_CATEGORIES: RetroCategory[] = ["WentWell", "WentWrong", "ActionItem"];

export const RETRO_CATEGORY_LABELS: Record<RetroCategory, string> = {
  WentWell: "Went Well",
  WentWrong: "Went Wrong",
  ActionItem: "Action Items",
};

export interface RetrospectiveNote {
  id: string;
  sprintId: string;
  category: RetroCategory;
  text: string;
  createdByUserId: string;
  createdByName: string;
  createdAt: string;
}

export interface RetrospectiveNotesResponse {
  notes: RetrospectiveNote[];
}

export interface CreateRetrospectiveNoteRequest {
  category: RetroCategory;
  text: string;
}

// ---------- Gantt ----------

export interface GanttDependencyRef {
  dependsOnWorkItemId: string;
  dependencyType: string;
}

export interface GanttItem {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  progress: number;
  status: WorkItemStatus;
  priority: WorkItemPriority;
  parentWorkItemId?: string | null;
  dependencies: GanttDependencyRef[];
}

export interface GanttMilestone {
  id: string;
  name: string;
  dueDate: string;
}

export interface GanttChart {
  items: GanttItem[];
  milestones: GanttMilestone[];
}

export interface CriticalPath {
  workItemIds: string[];
}

export interface GanttBaseline {
  id: string;
  projectId: string;
  name: string;
  createdAt: string;
  itemCount: number;
}

export interface GanttBaselineItem {
  workItemId: string;
  title: string;
  plannedStartDate: string;
  plannedEndDate: string;
}

export interface GanttBaselineDetail {
  id: string;
  name: string;
  createdAt: string;
  items: GanttBaselineItem[];
}

export interface CreateBaselineRequest {
  name: string;
}

// ---------- Calendar ----------

export type CalendarEventType = "WorkItemDue" | "Milestone" | "SprintStart" | "SprintEnd";

export interface CalendarEvent {
  id: string;
  type: CalendarEventType;
  title: string;
  date: string;
  endDate?: string | null;
  projectId?: string | null;
  projectName?: string | null;
}

export interface CalendarEventsResponse {
  events: CalendarEvent[];
}

export interface CalendarQueryParams {
  organizationId: string;
  projectId?: string;
  from?: string;
  to?: string;
}

export interface WorkloadRow {
  userId: string;
  userName: string;
  date: string;
  allocatedHours: number;
}

export interface CalendarWorkloadResponse {
  rows: WorkloadRow[];
}

// ---------- Time tracking ----------

export interface ActiveTimer {
  id: string;
  workItemId: string;
  workItemTitle: string;
  startedAt: string;
}

export interface StartTimerRequest {
  workItemId: string;
}

export interface TimesheetEntry {
  id: string;
  date: string;
  workItemId: string;
  workItemTitle: string;
  projectName: string;
  minutes: number;
  isBillable: boolean;
}

export interface TimesheetResponse {
  entries: TimesheetEntry[];
  totalMinutes: number;
  billableMinutes: number;
}

export interface TimesheetQueryParams {
  userId: string;
  from?: string;
  to?: string;
}

// ---------- Chat ----------

export interface ChatChannel {
  id: string;
  organizationId: string;
  projectId?: string | null;
  name: string;
  isPrivate: boolean;
  memberCount: number;
  unreadCount: number;
  createdAt: string;
}

export interface CreateChatChannelRequest {
  organizationId: string;
  projectId?: string | null;
  name: string;
  isPrivate: boolean;
  memberUserIds: string[];
}

export interface ChatReaction {
  emoji: string;
  userIds: string[];
}

export interface ChatAttachment {
  id: string;
  fileName: string;
  downloadUrl: string;
  fileSizeBytes: number;
}

export interface ChatMessage {
  id: string;
  channelId?: string | null;
  directConversationId?: string | null;
  authorUserId: string;
  authorName: string;
  body: string;
  parentMessageId?: string | null;
  createdAt: string;
  editedAt?: string | null;
  isDeleted: boolean;
  reactions: ChatReaction[];
  attachments: ChatAttachment[];
  replyCount: number;
}

export interface ChatMessagesResponse {
  messages: ChatMessage[];
}

export interface SendChatMessageRequest {
  body: string;
  parentMessageId?: string | null;
}

export interface DirectConversation {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserOnline: boolean;
  lastMessagePreview?: string | null;
  lastMessageAt?: string | null;
  unreadCount: number;
}

export interface CreateConversationRequest {
  otherUserId: string;
}

// Unified reference to either a channel or a direct conversation thread, used throughout
// the chat UI/hooks so components don't need two parallel code paths.
export type ChatThreadRef =
  | { kind: "channel"; id: string }
  | { kind: "conversation"; id: string };

// ---------- Notifications ----------

export type NotificationChannel = "InApp" | "Email" | "Slack" | "Teams" | "Sms";

export const NOTIFICATION_CHANNELS: NotificationChannel[] = [
  "InApp",
  "Email",
  "Slack",
  "Teams",
  "Sms",
];

export interface AppNotification {
  id: string;
  type: string;
  title: string;
  body: string;
  linkUrl?: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationListParams {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean;
}

export interface NotificationPreference {
  channel: NotificationChannel;
  isEnabled: boolean;
}

export interface IntegrationSettings {
  slackWebhookUrl?: string | null;
  teamsWebhookUrl?: string | null;
  smsProviderUrl?: string | null;
}

export interface UpdateIntegrationSettingsRequest extends IntegrationSettings {
  smsProviderApiKey?: string;
}

// ---------- Documents / Wiki ----------

export type DocPageScope = "ProjectDocument" | "OrgWiki";

export type DocPageCategory =
  | "KnowledgeBase"
  | "ApiDocs"
  | "MeetingNotes"
  | "Architecture"
  | "ReleaseNotes"
  | "General";

export const DOC_PAGE_CATEGORIES: DocPageCategory[] = [
  "KnowledgeBase",
  "ApiDocs",
  "MeetingNotes",
  "Architecture",
  "ReleaseNotes",
  "General",
];

export const DOC_PAGE_CATEGORY_LABELS: Record<DocPageCategory, string> = {
  KnowledgeBase: "Knowledge Base",
  ApiDocs: "API Docs",
  MeetingNotes: "Meeting Notes",
  Architecture: "Architecture",
  ReleaseNotes: "Release Notes",
  General: "General",
};

export interface DocPageSummary {
  id: string;
  title: string;
  scope: DocPageScope;
  category?: DocPageCategory | null;
  parentPageId?: string | null;
  updatedAt: string;
  updatedByName: string;
}

export interface DocPageDetail {
  id: string;
  organizationId: string;
  projectId?: string | null;
  scope: DocPageScope;
  category?: DocPageCategory | null;
  title: string;
  content: string;
  parentPageId?: string | null;
  createdByUserId: string;
  createdByName: string;
  updatedByName: string;
  createdAt: string;
  updatedAt: string;
  versionCount: number;
}

export interface DocPageListParams {
  organizationId: string;
  projectId?: string;
  scope: DocPageScope;
  category?: DocPageCategory;
  parentPageId?: string;
}

export interface CreateDocPageRequest {
  organizationId: string;
  projectId?: string | null;
  scope: DocPageScope;
  category?: DocPageCategory | null;
  title: string;
  content: string;
  parentPageId?: string | null;
}

export interface UpdateDocPageRequest {
  title: string;
  content: string;
  parentPageId?: string | null;
  category?: DocPageCategory | null;
}

export interface DocPageVersionSummary {
  id: string;
  versionNumber: number;
  editedByName: string;
  createdAt: string;
}

export interface DocPageVersionDetail {
  versionNumber: number;
  content: string;
  editedByName: string;
  createdAt: string;
}

export interface DocPageComment {
  id: string;
  authorUserId: string;
  authorName: string;
  body: string;
  createdAt: string;
}

export interface CreateDocPageCommentRequest {
  body: string;
}

// ---------- Dashboard ----------

// The dashboard aggregates tasks across many projects, so — unlike the single-project
// Kanban board — each card also needs to know which project it belongs to so the UI can
// deep-link into that project's board. Extends the Phase-2 KanbanCard rather than
// duplicating it; projectId/projectName are optional so the UI still degrades gracefully
// (falls back to the projects list) if the backend ever omits them.
export interface DashboardTaskCard extends KanbanCard {
  projectId?: string;
  projectName?: string;
}

export interface RecentActivityItem {
  id: string;
  message: string;
  linkUrl?: string | null;
  createdAt: string;
}

export interface ActiveSprintSummary {
  sprintId: string;
  sprintName: string;
  projectId: string;
  projectName: string;
  progressPercent: number;
  daysRemaining: number;
}

export interface DashboardResponse {
  todaysTasks: DashboardTaskCard[];
  overdueTasks: DashboardTaskCard[];
  assignedTaskCount: number;
  recentActivity: RecentActivityItem[];
  activeSprints: ActiveSprintSummary[];
  unreadNotificationCount: number;
}

// ---------- Reports ----------

export type ProjectHealthStatus = "Green" | "Yellow" | "Red";

export interface ProjectHealthSummary {
  projectId: string;
  projectName: string;
  health: ProjectHealthStatus;
  progressPercent: number;
  overdueCount: number;
  activeSprintName?: string | null;
}

export interface ExecutiveDashboardReport {
  totalProjects: number;
  activeProjects: number;
  totalWorkItems: number;
  completedThisMonth: number;
  overdueCount: number;
  atRiskProjectCount: number;
  projectHealthSummaries: ProjectHealthSummary[];
}

export interface ExecutiveDashboardParams {
  organizationId: string;
}

export interface SprintReport {
  sprint: Sprint;
  burndown: BurndownData;
  blockedItems: KanbanCard[];
  openRetroActionItemCount: number;
}

export interface CycleTimeItem {
  workItemId: string;
  title: string;
  leadTimeHours: number;
  cycleTimeHours: number;
}

export interface CycleTimeReport {
  averageLeadTimeHours: number | null;
  averageCycleTimeHours: number | null;
  items: CycleTimeItem[];
}

export interface CycleTimeParams {
  projectId: string;
  from?: string;
  to?: string;
}

export interface ProjectHealthReport {
  status: ProjectHealthStatus;
  reasons: string[];
  overdueCount: number;
  blockedCount: number;
  sprintProgressPercent: number | null;
}

export interface ProductivityBucket {
  periodStart: string;
  completedCount: number;
  completedPoints: number;
}

export interface ProductivityReport {
  buckets: ProductivityBucket[];
}

export type ProductivityBucketSize = "week" | "month";

export interface ProductivityParams {
  projectId: string;
  from?: string;
  to?: string;
  bucket: ProductivityBucketSize;
}

export interface ResourceUtilizationRow {
  userId: string;
  userName: string;
  estimatedHours: number;
  loggedHours: number;
  taskCount: number;
  utilizationPercent: number | null;
}

export interface ResourceUtilizationReport {
  rows: ResourceUtilizationRow[];
}

export interface ResourceUtilizationParams {
  organizationId: string;
  projectId?: string;
  from?: string;
  to?: string;
}

export interface CostAnalysisByUser {
  userId: string;
  userName: string;
  billableHours: number;
  cost: number;
}

export interface CostAnalysisReport {
  totalBillableHours: number;
  totalNonBillableHours: number;
  totalCost: number;
  hourlyRateConfigured: boolean;
  byUser: CostAnalysisByUser[];
}

export interface CostAnalysisParams {
  projectId: string;
  from?: string;
  to?: string;
}

// ---------- AI (Phase 6) ----------
// Base path `/ai`. Every endpoint here can return a 503 ProblemDetails when no AI
// provider is configured — see utils/apiErrors.ts#isAiUnconfiguredError.

export interface GenerateTasksRequest {
  projectId: string;
  prompt: string;
}

export interface TaskSuggestion {
  title: string;
  description: string;
  priority: WorkItemPriority;
  type: WorkItemType;
  storyPoints: number | null;
}

export interface GenerateTasksResponse {
  suggestions: TaskSuggestion[];
}

export interface PlanSprintRequest {
  projectId: string;
  sprintGoal: string;
  capacityPoints: number;
}

export interface PlanSprintResponse {
  selectedWorkItemIds: string[];
  reasoning: string;
}

export interface AnalyzeBugRequest {
  workItemId: string;
}

export interface AnalyzeBugResponse {
  probableRootCause: string;
  reproSteps: string;
  suggestedSeverity: string;
  reasoning: string;
}

export interface SummarizeMeetingRequest {
  rawNotes: string;
  projectId: string;
  saveAsWikiPage: boolean;
}

export interface MeetingActionItem {
  text: string;
}

export interface SummarizeMeetingResponse {
  summary: string;
  actionItems: MeetingActionItem[];
  wikiPageId: string | null;
}

export interface AiChatRequest {
  projectId: string;
  message: string;
  conversationId: string | null;
}

export interface AiChatResponse {
  conversationId: string;
  reply: string;
}

export interface AiConversationSummary {
  id: string;
  title: string;
  createdAt: string;
  lastMessageAt: string;
}

export type AiChatMessageRole = "User" | "Assistant";

export interface AiChatMessage {
  id: string;
  role: AiChatMessageRole;
  content: string;
  createdAt: string;
}

export type RiskLevel = "Low" | "Medium" | "High";

export interface RiskPredictionResponse {
  riskLevel: RiskLevel;
  explanation: string;
}

export interface ResourceAllocationSuggestion {
  userId: string;
  userName: string;
  recommendation: string;
}

export interface ResourceAllocationResponse {
  suggestions: ResourceAllocationSuggestion[];
  reasoning: string;
}

export interface EstimateStoryPointsRequest {
  title: string;
  description: string;
}

export interface EstimateStoryPointsResponse {
  suggestedPoints: number;
  reasoning: string;
}

export type PredictionConfidence = "Low" | "Medium" | "High";

export interface DeadlinePredictionResponse {
  predictedDate: string;
  confidence: PredictionConfidence;
  reasoning: string;
}

export interface PrioritizeTasksRequest {
  projectId: string;
}

export interface PrioritizedTaskSuggestion {
  workItemId: string;
  suggestedPriority: WorkItemPriority;
  reasoning: string;
}

export interface PrioritizeTasksResponse {
  items: PrioritizedTaskSuggestion[];
}

export type CodeReviewSeverity = "Info" | "Warning" | "Critical";

export interface ReviewCodeRequest {
  code: string;
  language: string;
}

export interface CodeReviewSuggestion {
  line: number;
  comment: string;
  severity: CodeReviewSeverity;
}

export interface ReviewCodeResponse {
  suggestions: CodeReviewSuggestion[];
  summary: string;
}

export interface GenerateReleaseNotesRequest {
  projectId: string;
  from: string;
  to: string;
  saveAsWikiPage: boolean;
}

export interface GenerateReleaseNotesResponse {
  markdown: string;
  wikiPageId: string | null;
}

// ---------- Automation (Phase 6) ----------
// Base path `/automation`.

export type WorkflowTriggerType =
  | "WorkItemCreated"
  | "WorkItemStatusChanged"
  | "WorkItemAssigned"
  | "SprintStarted"
  | "Scheduled";

export const WORKFLOW_TRIGGER_TYPES: WorkflowTriggerType[] = [
  "WorkItemCreated",
  "WorkItemStatusChanged",
  "WorkItemAssigned",
  "SprintStarted",
  "Scheduled",
];

export const WORKFLOW_TRIGGER_LABELS: Record<WorkflowTriggerType, string> = {
  WorkItemCreated: "Work item created",
  WorkItemStatusChanged: "Work item status changed",
  WorkItemAssigned: "Work item assigned",
  SprintStarted: "Sprint started",
  Scheduled: "Scheduled (cron)",
};

export type WorkflowConditionOperator = "Equals" | "NotEquals" | "GreaterThan" | "LessThan" | "Contains";

export const WORKFLOW_CONDITION_OPERATORS: WorkflowConditionOperator[] = [
  "Equals",
  "NotEquals",
  "GreaterThan",
  "LessThan",
  "Contains",
];

export type WorkflowActionType =
  | "ChangeStatus"
  | "AssignUser"
  | "AddLabel"
  | "SendNotification"
  | "CallWebhook"
  | "RequireApproval";

export const WORKFLOW_ACTION_TYPES: WorkflowActionType[] = [
  "ChangeStatus",
  "AssignUser",
  "AddLabel",
  "SendNotification",
  "CallWebhook",
  "RequireApproval",
];

export const WORKFLOW_ACTION_LABELS: Record<WorkflowActionType, string> = {
  ChangeStatus: "Change status",
  AssignUser: "Assign user",
  AddLabel: "Add label",
  SendNotification: "Send notification",
  CallWebhook: "Call webhook",
  RequireApproval: "Require approval",
};

export interface WorkflowSummary {
  id: string;
  name: string;
  triggerType: WorkflowTriggerType;
  isEnabled: boolean;
  conditionCount: number;
  actionCount: number;
  createdAt: string;
}

export interface WorkflowCondition {
  id?: string;
  fieldPath: string;
  operator: WorkflowConditionOperator;
  value: string;
}

export interface WorkflowAction {
  id?: string;
  actionType: WorkflowActionType;
  actionConfigJson: string;
  order: number;
}

export interface CreateWorkflowRequest {
  organizationId: string;
  projectId: string;
  name: string;
  triggerType: WorkflowTriggerType;
  cronExpression?: string | null;
  conditions: WorkflowCondition[];
  actions: WorkflowAction[];
}

export interface UpdateWorkflowRequest {
  name: string;
  conditions: WorkflowCondition[];
  actions: WorkflowAction[];
}

export interface WorkflowDetail {
  id: string;
  organizationId: string;
  projectId: string;
  name: string;
  triggerType: WorkflowTriggerType;
  cronExpression?: string | null;
  isEnabled: boolean;
  conditions: WorkflowCondition[];
  actions: WorkflowAction[];
  createdAt: string;
}

export type WorkflowRunStatus = "Running" | "Succeeded" | "Failed" | "AwaitingApproval";

export interface WorkflowRun {
  id: string;
  status: WorkflowRunStatus;
  triggerEntityId: string;
  startedAt: string;
  completedAt?: string | null;
  logJson: string;
}

export interface WorkflowRunsParams {
  workflowId: string;
  page?: number;
  pageSize?: number;
}

export type ApprovalStatus = "AwaitingApproval" | "Approved" | "Rejected";

export interface Approval {
  id: string;
  workflowRunId: string;
  workflowName: string;
  status: ApprovalStatus;
  requestedAt: string;
}

export interface DecideApprovalRequest {
  approve: boolean;
  comment: string;
}

// Per-action-type config shapes serialized into WorkflowAction.actionConfigJson by the
// workflow editor. Not sent as-is over the wire — always JSON.stringify'd first.
export interface ChangeStatusActionConfig {
  status: WorkItemStatus;
}

export interface AssignUserActionConfig {
  userId: string;
}

export interface AddLabelActionConfig {
  labelId: string;
}

export interface SendNotificationActionConfig {
  message: string;
}

export interface CallWebhookActionConfig {
  url: string;
  method: string;
}

export interface RequireApprovalActionConfig {
  approverUserId: string;
}
