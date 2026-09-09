import axios from 'axios'
import { useAuthStore } from '../store/auth'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api/lineagestudio'
export const HUB_BASE_URL = import.meta.env.VITE_HUB_BASE_URL ?? 'http://localhost:5000/hubs/lineagestudio'

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !error.config?.url?.includes('/auth/login')) {
      // An expired/invalid token - clear it so the route guard sends the user back to /login,
      // rather than leaving every page silently failing.
      useAuthStore.getState().logout()
    }
    return Promise.reject(error)
  },
)
