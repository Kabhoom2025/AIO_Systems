export interface Organization {
  id: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  logoUrl?: string;
  isActive: boolean;
  timezone?: string;
  currency?: string;
  userCount: number;
  createdDate: string;
}

export interface CreateOrganizationRequest {
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  logoUrl?: string;
  timezone?: string;
  currency?: string;
}

export interface UpdateOrganizationRequest extends CreateOrganizationRequest {
  isActive: boolean;
}

export interface OrgUser {
  id: number;
  name: string;
  email: string;
  roleName: string;
  branchId?: number;
  branchName?: string;
  isActive: boolean;
  createdDate: string;
}

export interface CreateOrgAdminRequest {
  name: string;
  email: string;
  password: string;
  branchId?: number | null;
}

export interface License {
  id: number;
  organizationId: number;
  orgName: string;
  plan: 'Basic' | 'Pro' | 'Enterprise';
  status: 'Active' | 'Trial' | 'Expired';
  expiryDate: string;
  maxUsers: number;
  notes?: string;
  createdDate: string;
}

export interface UpsertLicenseRequest {
  organizationId: number;
  plan: 'Basic' | 'Pro' | 'Enterprise';
  status: 'Active' | 'Trial' | 'Expired';
  expiryDate: string;
  maxUsers: number;
  notes?: string;
}

export interface OrgReport {
  organizationId: number;
  orgName: string;
  totalOrders: number;
  totalRevenue: number;
  activeUsers: number;
  todayOrders: number;
  todayRevenue: number;
  thisMonthOrders: number;
  thisMonthRevenue: number;
}
