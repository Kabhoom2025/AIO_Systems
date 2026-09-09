import { Injectable } from '@angular/core';

const ACCESS_TOKEN_KEY = 'chatbot.accessToken';
const REFRESH_TOKEN_KEY = 'chatbot.refreshToken';

/**
 * Wraps localStorage so a corrupted/blocked storage (private browsing, disabled cookies)
 * degrades to an in-memory fallback instead of crashing the app.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private memoryAccessToken: string | null = null;
  private memoryRefreshToken: string | null = null;

  getAccessToken(): string | null {
    try {
      return localStorage.getItem(ACCESS_TOKEN_KEY);
    } catch {
      return this.memoryAccessToken;
    }
  }

  getRefreshToken(): string | null {
    try {
      return localStorage.getItem(REFRESH_TOKEN_KEY);
    } catch {
      return this.memoryRefreshToken;
    }
  }

  setTokens(accessToken: string, refreshToken: string): void {
    this.memoryAccessToken = accessToken;
    this.memoryRefreshToken = refreshToken;
    try {
      localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    } catch {
      // fall back silently to the in-memory copy already set above
    }
  }

  clear(): void {
    this.memoryAccessToken = null;
    this.memoryRefreshToken = null;
    try {
      localStorage.removeItem(ACCESS_TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    } catch {
      // ignore
    }
  }
}
