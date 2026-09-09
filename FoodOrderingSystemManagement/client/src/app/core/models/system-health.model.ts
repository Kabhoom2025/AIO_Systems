export interface HealthEventDto {
  timestamp: string;
  level: 'INFO' | 'WARN' | 'ERROR';
  category: string;
  message: string;
}

export interface SystemHealthDto {
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
  recentEvents: HealthEventDto[];
}
