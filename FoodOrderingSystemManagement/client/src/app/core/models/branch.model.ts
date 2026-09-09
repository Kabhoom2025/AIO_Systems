export interface Branch {
  id: number;
  organizationId: number;
  name: string;
  address?: string;
  phone?: string;
  isActive: boolean;
  isDefault: boolean;
  createdDate: string;
}

export interface CreateBranchRequest {
  name: string;
  address?: string;
  phone?: string;
  organizationId?: number;
}

export interface UpdateBranchRequest {
  name: string;
  address?: string;
  phone?: string;
  isActive: boolean;
}

export interface BranchReport {
  branchId: number;
  branchName: string;
  totalOrders: number;
  totalRevenue: number;
  todayOrders: number;
  todayRevenue: number;
  thisMonthOrders: number;
  thisMonthRevenue: number;
}
