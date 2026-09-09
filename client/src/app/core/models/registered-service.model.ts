export interface RegisteredService {
  id: number;
  name: string;
  routePrefix: string;
  baseUrl: string;
  description?: string | null;
  moduleKeys: string;
  isActive: boolean;
  healthCheckPath?: string | null;
  lastVerifiedAt?: string | null;
  createdDate: string;

  backendWorkingDirectory?: string | null;
  backendCommand?: string | null;
  backendRunning: boolean;

  frontendUrl?: string | null;
  frontendWorkingDirectory?: string | null;
  frontendCommand?: string | null;
  frontendRunning: boolean;

  /** Docker Compose service name for the backend (e.g. "hrms-api"). Null/empty = Docker mode unavailable for this app. */
  dockerServiceName?: string | null;

  icon: string;
  color: string;
  sortOrder: number;
}

export type RunMode = 'Native' | 'Docker';

export interface CreateRegisteredServiceRequest {
  name: string;
  routePrefix: string;
  baseUrl: string;
  description?: string | null;
  moduleKeys: string;
  healthCheckPath?: string | null;

  backendWorkingDirectory?: string | null;
  backendCommand?: string | null;
  frontendUrl?: string | null;
  frontendWorkingDirectory?: string | null;
  frontendCommand?: string | null;
  dockerServiceName?: string | null;

  icon: string;
  color: string;
  sortOrder: number;
}

export interface UpdateRegisteredServiceRequest extends CreateRegisteredServiceRequest {
  isActive: boolean;
}
