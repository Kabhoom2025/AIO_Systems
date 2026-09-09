import { environment } from '../../environments/environment';

export function getOrgIdFromToken(): number {
  const token = localStorage.getItem(environment.tokenKey);
  if (!token) return 0;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    // The restaurant API puts orgId in a custom claim
    const orgId = payload['organizationId'];
    return orgId ? Number(orgId) : 0;
  } catch {
    return 0;
  }
}

export function getBranchIdFromToken(): number | null {
  const token = localStorage.getItem(environment.tokenKey);
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const branchId = payload['branchId'];
    return branchId ? Number(branchId) : null;
  } catch {
    return null;
  }
}

export function getAuthHeaders(): Record<string, string> {
  const token = localStorage.getItem(environment.tokenKey);
  return token ? { Authorization: `Bearer ${token}` } : {};
}
