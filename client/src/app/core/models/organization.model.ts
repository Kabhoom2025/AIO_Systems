export interface Organization {
  id: number;
  name: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  logoUrl?: string | null;
  isActive: boolean;
  timezone?: string | null;
  currency?: string | null;
  tenantKey: string;
  userCount: number;
  createdDate: string;
}

export interface CreateOrganizationRequest {
  name: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  logoUrl?: string | null;
  timezone?: string | null;
  currency?: string | null;
}

export interface UpdateOrganizationRequest extends CreateOrganizationRequest {
  isActive: boolean;
}

export interface OrgUser {
  id: number;
  name: string;
  email: string;
  roleName: string;
  isActive: boolean;
  createdDate: string;
}

export interface CreateOrgAdminRequest {
  name: string;
  email: string;
  password: string;
}

export interface License {
  id: number;
  organizationId: number;
  orgName: string;
  plan: string;
  status: string;
  expiryDate: string;
  maxUsers: number;
  notes?: string | null;
  createdDate: string;
}

export interface UpsertLicenseRequest {
  organizationId: number;
  plan: string;
  status: string;
  expiryDate: string;
  maxUsers: number;
  notes?: string | null;
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
