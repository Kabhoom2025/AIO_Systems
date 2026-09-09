import { axiosClient } from "./axiosClient";
import type {
  AppNotification,
  IntegrationSettings,
  NotificationListParams,
  NotificationPreference,
  PagedResult,
  UpdateIntegrationSettingsRequest,
} from "../types";

export const notificationsApi = {
  list: (params: NotificationListParams) =>
    axiosClient
      .get<PagedResult<AppNotification>>("/notifications", { params })
      .then((r) => r.data),

  markRead: (id: string) =>
    axiosClient.post<void>(`/notifications/${id}/read`).then((r) => r.data),

  markAllRead: () => axiosClient.post<void>("/notifications/read-all").then((r) => r.data),

  unreadCount: () =>
    axiosClient.get<{ count: number }>("/notifications/unread-count").then((r) => r.data),

  getPreferences: () =>
    axiosClient.get<NotificationPreference[]>("/notification-preferences").then((r) => r.data),

  updatePreference: (payload: NotificationPreference) =>
    axiosClient.put<void>("/notification-preferences", payload).then((r) => r.data),

  getIntegrationSettings: (organizationId: string) =>
    axiosClient
      .get<IntegrationSettings>(`/organizations/${organizationId}/integration-settings`)
      .then((r) => r.data),

  updateIntegrationSettings: (organizationId: string, payload: UpdateIntegrationSettingsRequest) =>
    axiosClient
      .put<void>(`/organizations/${organizationId}/integration-settings`, payload)
      .then((r) => r.data),
};
