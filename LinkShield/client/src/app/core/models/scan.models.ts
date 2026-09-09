export type ScanStatus =
  | 'Queued' | 'UrlAnalysis' | 'DomainAnalysis' | 'DnsAnalysis' | 'SslAnalysis'
  | 'RedirectAnalysis' | 'ThreatIntelligence' | 'BrandDetection' | 'MlAnalysis'
  | 'RiskScoring' | 'Completed' | 'Failed';

export type RiskLevel = 'Low' | 'Moderate' | 'High' | 'Critical';
export type ScanVerdict = 'Unknown' | 'Safe' | 'LowRisk' | 'Suspicious' | 'Malicious';

export interface UrlAnalysisResult {
  originalUrl: string;
  normalizedUrl: string;
  urlLength: number;
  domainLength: number;
  pathLength: number;
  queryLength: number;
  subdomainCount: number;
  dotCount: number;
  hyphenCount: number;
  digitCount: number;
  specialCharCount: number;
  isIpAddressHost: boolean;
  isHttps: boolean;
  hasUrlEncoding: boolean;
  hasDoubleEncoding: boolean;
  hasPunycode: boolean;
  hasHomoglyphs: boolean;
  isShortenedUrl: boolean;
  hasSuspiciousTld: boolean;
  hasRedirectParameter: boolean;
  hasSuspiciousFileExtension: boolean;
  suspiciousKeywords: string[];
  suspiciousParameters: string[];
  urlAnalysisScore: number;
}

export interface DomainAnalysisResult {
  domain: string;
  registeredDomain: string;
  subdomain: string;
  tld: string;
  registrar: string | null;
  registeredAtUtc: string | null;
  expiresAtUtc: string | null;
  domainAgeDays: number | null;
  organization: string | null;
  country: string | null;
  nameservers: string[];
  rdapLookupSucceeded: boolean;
  rdapError: string | null;
  domainAnalysisScore: number;
}

export interface DnsAnalysisResult {
  resolves: boolean;
  aRecords: string[];
  aaaaRecords: string[];
  mxRecords: string[];
  nsRecords: string[];
  cnameRecords: string[];
  txtRecords: string[];
  hasMailConfiguration: boolean;
  hasSuspiciousNameservers: boolean;
  lookupError: string | null;
  dnsAnalysisScore: number;
}

export interface SslAnalysisResult {
  hasCertificate: boolean;
  isValid: boolean;
  issuer: string | null;
  subject: string | null;
  subjectAlternativeNames: string[];
  validFromUtc: string | null;
  validToUtc: string | null;
  isExpired: boolean;
  isSelfSigned: boolean;
  domainMatchesCertificate: boolean;
  tlsVersion: string | null;
  chainValidationError: string | null;
  sslAnalysisScore: number;
}

export interface RedirectHop {
  hopIndex: number;
  fromUrl: string;
  toUrl: string;
  statusCode: number;
  domainChanged: boolean;
  httpsDowngrade: boolean;
  targetIsSuspicious: boolean;
}

export interface RedirectAnalysisResult {
  finalUrl: string;
  redirectCount: number;
  anyDomainChange: boolean;
  anyHttpsDowngrade: boolean;
  hops: RedirectHop[];
  error: string | null;
  redirectAnalysisScore: number;
}

export interface ThreatIntelligenceResult {
  providerName: string;
  providerSlug: string;
  status: 'NoThreatDetected' | 'Suspicious' | 'ConfirmedMalicious' | 'Error' | 'Unavailable';
  detectionCategory: string | null;
  errorMessage: string | null;
  fromCache: boolean;
  checkedAtUtc: string;
}

export interface BrandMatchResult {
  brandProfileId: string;
  brandName: string;
  officialDomain: string;
  matchedDomain: string;
  matchType: string;
  levenshteinDistance: number;
  jaroWinklerSimilarity: number;
  containsAuthKeywords: boolean;
}

export interface MlPredictionResult {
  prediction: string;
  probability: number;
  modelVersion: string;
}

export interface RiskFactor {
  category: string;
  description: string;
  scoreContribution: number;
}

export interface ScanDetail {
  scanId: string;
  originalUrl: string;
  normalizedUrl: string;
  status: ScanStatus;
  failureReason: string | null;
  riskScore: number;
  riskLevel: RiskLevel;
  verdict: ScanVerdict;
  confidence: number;
  urlAnalysis: UrlAnalysisResult | null;
  domainAnalysis: DomainAnalysisResult | null;
  dnsAnalysis: DnsAnalysisResult | null;
  sslAnalysis: SslAnalysisResult | null;
  redirectAnalysis: RedirectAnalysisResult | null;
  threatIntelligenceResults: ThreatIntelligenceResult[];
  brandMatches: BrandMatchResult[];
  mlPrediction: MlPredictionResult | null;
  riskFactors: RiskFactor[];
  createdAtUtc: string;
  completedAtUtc: string | null;
}

export interface ScanSummary {
  scanId: string;
  originalUrl: string;
  status: ScanStatus;
  riskScore: number;
  riskLevel: RiskLevel;
  verdict: ScanVerdict;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export const TERMINAL_STATUSES: ScanStatus[] = ['Completed', 'Failed'];
