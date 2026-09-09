import { API_BASE_URL } from "./axiosClient";

// The REST base URL is e.g. http://localhost:5000/api/projectflow. The gateway forwards
// /hubs/projectflow/{**catch-all} to the backend's /hubs/{**catch-all}, so hub URLs live on
// the same host but replace the /api/projectflow suffix with /hubs/projectflow/<hub>.
function hubBaseUrl(): string {
  return API_BASE_URL.replace(/\/api\/projectflow\/?$/, "");
}

export const CHAT_HUB_URL = `${hubBaseUrl()}/hubs/projectflow/chat`;
export const NOTIFICATIONS_HUB_URL = `${hubBaseUrl()}/hubs/projectflow/notifications`;
