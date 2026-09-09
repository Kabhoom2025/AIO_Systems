import { environment } from '../../environments/environment';

function decodeToken(): Record<string, unknown> | null {
  const token = localStorage.getItem(environment.tokenKey);
  if (!token) return null;
  try {
    return JSON.parse(atob(token.split('.')[1]));
  } catch {
    return null;
  }
}

export function getOrgIdFromToken(): number {
  const payload = decodeToken();
  const orgId = payload?.['organizationId'];
  return orgId ? Number(orgId) : 0;
}

export function getEmployeeIdFromToken(): number | null {
  const payload = decodeToken();
  const employeeId = payload?.['employeeId'];
  return employeeId ? Number(employeeId) : null;
}

export function getUserIdFromToken(): number {
  const payload = decodeToken();
  const sub = payload?.['sub'];
  return sub ? Number(sub) : 0;
}
