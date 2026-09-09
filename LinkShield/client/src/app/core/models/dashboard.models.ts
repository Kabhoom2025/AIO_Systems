export interface DashboardSummary {
  totalScans: number;
  safeCount: number;
  suspiciousCount: number;
  maliciousCount: number;
  criticalCount: number;
  threatsDetected: number;
  averageRiskScore: number;
  threatIntelligenceMatches: number;
}

export interface ScansOverTimePoint {
  date: string;
  totalScans: number;
  safeCount: number;
  suspiciousCount: number;
  maliciousCount: number;
}

export interface RiskDistributionPoint {
  riskLevel: string;
  count: number;
}

export interface ThreatCategoryPoint {
  category: string;
  count: number;
}

export interface TopDomainPoint {
  domain: string;
  count: number;
  averageRiskScore: number;
}

export interface TopBrandPoint {
  brandName: string;
  count: number;
}

export interface DashboardTrends {
  scansOverTime: ScansOverTimePoint[];
  riskDistribution: RiskDistributionPoint[];
  threatCategories: ThreatCategoryPoint[];
  topSuspiciousDomains: TopDomainPoint[];
  topTargetedBrands: TopBrandPoint[];
}
