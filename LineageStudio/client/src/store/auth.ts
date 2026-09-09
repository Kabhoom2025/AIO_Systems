import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { apiClient } from '../services/api'
import { resetConnection } from '../services/signalr'

interface AuthUser {
  userId: string
  email: string
  displayName: string
}

interface LoginResponse {
  token: string
  expiresAt: string
  userId: string
  email: string
  displayName: string
}

interface AuthState {
  token: string | null
  user: AuthUser | null
  expiresAt: string | null
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      expiresAt: null,
      isLoading: false,
      login: async (email, password) => {
        set({ isLoading: true })
        try {
          const { data } = await apiClient.post<LoginResponse>('/auth/login', { email, password })
          set({
            token: data.token,
            user: { userId: data.userId, email: data.email, displayName: data.displayName },
            expiresAt: data.expiresAt,
            isLoading: false,
          })
        } catch (err) {
          set({ isLoading: false })
          throw err
        }
      },
      logout: () => {
        set({ token: null, user: null, expiresAt: null })
        void resetConnection()
      },
    }),
    { name: 'lineagestudio-auth' },
  ),
)
