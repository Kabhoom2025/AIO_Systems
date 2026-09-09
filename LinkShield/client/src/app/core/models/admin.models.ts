export interface BrandProfile {
  id: string;
  brandName: string;
  officialDomain: string;
  aliasDomains: string[];
  isEnabled: boolean;
}

export interface CreateBrandProfileRequest {
  brandName: string;
  officialDomain: string;
  aliasDomains?: string[];
}

export type RiskFactorCategory =
  | 'UrlAnalysis' | 'DomainIntelligence' | 'DnsAnalysis' | 'SslAnalysis'
  | 'RedirectAnalysis' | 'ThreatIntelligence' | 'BrandImpersonation' | 'MachineLearning';

export interface RiskRule {
  id: string;
  name: string;
  description: string;
  category: RiskFactorCategory;
  weight: number;
  isEnabled: boolean;
  conditionExpression: string;
  scoreContribution: number;
}

export interface CreateRiskRuleRequest {
  name: string;
  description: string;
  category: RiskFactorCategory;
  weight: number;
  conditionExpression: string;
  scoreContribution: number;
}

export interface UpdateRiskRuleRequest {
  name: string;
  description: string;
  weight: number;
  isEnabled: boolean;
  conditionExpression: string;
  scoreContribution: number;
}

export interface ApiKeySummary {
  id: string;
  keyPrefix: string;
  label: string;
  expiresAtUtc: string | null;
  revokedAtUtc: string | null;
  lastUsedAtUtc: string | null;
  usageCount: number;
  isActive: boolean;
}

export interface ApiClient {
  id: string;
  organizationName: string;
  contactEmail: string;
  isActive: boolean;
  dailyQuota: number;
  requestsPerMinute: number;
  keys: ApiKeySummary[];
}

export interface CreateApiClientRequest {
  organizationName: string;
  contactEmail: string;
  dailyQuota?: number | null;
  requestsPerMinute?: number | null;
}

export interface CreateApiKeyRequest {
  label: string;
  expiresAtUtc?: string | null;
}

export interface CreatedApiKey {
  id: string;
  rawKey: string;
  keyPrefix: string;
  label: string;
}

export interface AuditLog {
  id: string;
  userId: string | null;
  userEmail: string | null;
  action: string;
  entityType: string;
  entityId: string | null;
  detailsJson: string | null;
  ipAddress: string | null;
  occurredAtUtc: string;
}
