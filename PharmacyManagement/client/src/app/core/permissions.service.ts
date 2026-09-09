import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private permissions = new Set<string>();

  constructor() {
    this.reload();
  }

  reload() {
    const token = localStorage.getItem(environment.tokenKey);
    this.permissions = new Set<string>();
    if (!token) return;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const raw: string = payload['permissions'] ?? '';
      raw.split(',').filter(Boolean).forEach(key => this.permissions.add(key));
    } catch {
      // leave permissions empty on decode failure
    }
  }

  has(key: string): boolean {
    return this.permissions.has(key);
  }

  hasAny(keys: string[]): boolean {
    return keys.some(k => this.permissions.has(k));
  }
}
