export interface Role {
  id: number;
  roleName: string;
  userCount: number;
}

export interface CreateRoleRequest {
  roleName: string;
}
