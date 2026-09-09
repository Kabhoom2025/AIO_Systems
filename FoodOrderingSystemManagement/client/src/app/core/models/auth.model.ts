export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userId: number;
  roleId: number;
  name: string;
  email: string;
  roleName: string;
  organizationId?: number;
  organizationName?: string;
  isSuperAdmin: boolean;
  profileImage?: string | null;
  enabledModules?: string[];
}
