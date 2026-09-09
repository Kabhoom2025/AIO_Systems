import { axiosClient } from "./axiosClient";
import type {
  AuthResponse,
  ForgotPasswordRequest,
  GoogleOAuthRequest,
  LoginRequest,
  LoginResponse,
  MicrosoftOAuthRequest,
  RefreshRequest,
  RefreshResponse,
  RegisterRequest,
  ResetPasswordRequest,
  TwoFactorEnableResponse,
  TwoFactorVerifyRequest,
  User,
  VerifyEmailRequest,
} from "../types";

export const authApi = {
  register: (payload: RegisterRequest) =>
    axiosClient.post<AuthResponse>("/auth/register", payload).then((r) => r.data),

  login: (payload: LoginRequest) =>
    axiosClient.post<LoginResponse>("/auth/login", payload).then((r) => r.data),

  verifyTwoFactor: (payload: TwoFactorVerifyRequest) =>
    axiosClient.post<AuthResponse>("/auth/2fa/verify", payload).then((r) => r.data),

  refresh: (payload: RefreshRequest) =>
    axiosClient.post<RefreshResponse>("/auth/refresh", payload).then((r) => r.data),

  logout: (payload: RefreshRequest) =>
    axiosClient.post<void>("/auth/logout", payload).then((r) => r.data),

  verifyEmail: (payload: VerifyEmailRequest) =>
    axiosClient.post<void>("/auth/verify-email", payload).then((r) => r.data),

  forgotPassword: (payload: ForgotPasswordRequest) =>
    axiosClient.post<void>("/auth/forgot-password", payload).then((r) => r.data),

  resetPassword: (payload: ResetPasswordRequest) =>
    axiosClient.post<void>("/auth/reset-password", payload).then((r) => r.data),

  enableTwoFactor: () =>
    axiosClient.post<TwoFactorEnableResponse>("/auth/2fa/enable").then((r) => r.data),

  me: () => axiosClient.get<User>("/auth/me").then((r) => r.data),

  oauthGoogle: (payload: GoogleOAuthRequest) =>
    axiosClient.post<LoginResponse>("/auth/oauth/google", payload).then((r) => r.data),

  oauthMicrosoft: (payload: MicrosoftOAuthRequest) =>
    axiosClient.post<LoginResponse>("/auth/oauth/microsoft", payload).then((r) => r.data),
};
