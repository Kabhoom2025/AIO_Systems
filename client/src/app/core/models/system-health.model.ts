export interface HealthEvent {
  timestamp: string;
  level: string;
  category: string;
  message: string;
}

export interface SystemHealth {
  apiStatus: string;
  dbStatus: string;
  dbLatencyMs: number;
  memoryUsedMb: number;
  gcHeapMb: number;
  uptime: string;
  machineName: string;
  osDescription: string;
  dotNetVersion: string;
  processId: number;
  threadCount: number;
  recentEvents: HealthEvent[];
}
