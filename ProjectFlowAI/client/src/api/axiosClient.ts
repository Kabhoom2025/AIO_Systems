import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { authStoreApi } from "../store/authStore";
import type { RefreshResponse } from "../types";

export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000/api/projectflow";

export const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Separate instance (no interceptors) to avoid infinite refresh loops.
const refreshClient = axios.create({ baseURL: API_BASE_URL });

axiosClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = authStoreApi.getState().accessToken;
  if (token) {
    config.headers.set("Authorization", `Bearer ${token}`);
  }
  return config;
});

interface RetriableConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let isRefreshing = false;
let pendingQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

function flushQueue(error: unknown, token: string | null) {
  pendingQueue.forEach(({ resolve, reject }) => {
    if (error || !token) {
      reject(error);
    } else {
      resolve(token);
    }
  });
  pendingQueue = [];
}

function forceLogout() {
  authStoreApi.setState({
    accessToken: null,
    refreshToken: null,
    user: null,
    isAuthenticated: false,
  });
  if (typeof window !== "undefined") {
    window.location.href = "/login";
  }
}

axiosClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableConfig | undefined;

    if (
      error.response?.status !== 401 ||
      !originalRequest ||
      originalRequest._retry ||
      originalRequest.url?.includes("/auth/refresh")
    ) {
      return Promise.reject(error);
    }

    const refreshToken = authStoreApi.getState().refreshToken;
    if (!refreshToken) {
      forceLogout();
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        pendingQueue.push({
          resolve: (token) => {
            originalRequest._retry = true;
            originalRequest.headers.set("Authorization", `Bearer ${token}`);
            resolve(axiosClient(originalRequest));
          },
          reject,
        });
      });
    }

    isRefreshing = true;
    originalRequest._retry = true;

    try {
      const { data } = await refreshClient.post<RefreshResponse>("/auth/refresh", {
        refreshToken,
      });
      authStoreApi.setState({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      });
      flushQueue(null, data.accessToken);
      originalRequest.headers.set("Authorization", `Bearer ${data.accessToken}`);
      return axiosClient(originalRequest);
    } catch (refreshError) {
      flushQueue(refreshError, null);
      forceLogout();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);
