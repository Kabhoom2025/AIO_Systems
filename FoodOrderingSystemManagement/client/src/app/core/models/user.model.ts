export interface AppUser {
  id: number;
  name: string;
  email: string;
  roleId: number;
  roleName: string;
  isActive: boolean;
  createdDate: string;
  profileImage?: string | null;
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  roleId: number;
  profileImage?: string | null;
}

export interface UpdateUserRequest {
  name: string;
  email: string;
  roleId: number;
  profileImage?: string | null;
}
