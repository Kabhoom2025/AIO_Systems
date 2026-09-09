export interface PlatformModule {
  id: number;
  name: string;
  key: string;
  description: string;
  icon: string;
  color: string;
  isActive: boolean;
  sortOrder: number;
}

export interface CreatePlatformModuleRequest {
  name: string;
  key: string;
  description: string;
  icon: string;
  color: string;
  sortOrder: number;
}

export interface UpdatePlatformModuleRequest {
  name: string;
  description: string;
  icon: string;
  color: string;
  isActive: boolean;
  sortOrder: number;
}

export interface OrgModuleItem {
  moduleId: number;
  moduleName: string;
  moduleKey: string;
  icon: string;
  color: string;
  isEnabled: boolean;
}

export interface OrgModuleStatus {
  organizationId: number;
  organizationName: string;
  modules: OrgModuleItem[];
}

export interface OrgModuleAssignmentRequest {
  organizationId: number;
  moduleIds: number[];
}
