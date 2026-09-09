export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: number;
  roleId: number;
  name: string;
  email: string;
  roleName: string;
  token: string;
  expiresAt: string;
  organizationId?: number | null;
  organizationName?: string | null;
  isSuperAdmin: boolean;
  profileImage?: string | null;
  enabledModules: string[];
}
