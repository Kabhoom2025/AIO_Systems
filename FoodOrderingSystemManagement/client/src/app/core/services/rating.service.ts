import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RatingStats, RecommendedItem, SubmitRatingRequest } from '../models/rating.model';

interface ApiResponse<T> { data: T; message?: string; }

@Injectable({ providedIn: 'root' })
export class RatingService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/rating`;

  getAllStats(): Observable<ApiResponse<RatingStats[]>> {
    return this.http.get<ApiResponse<RatingStats[]>>(`${this.base}/stats`);
  }

  getRecommended(): Observable<ApiResponse<RecommendedItem[]>> {
    return this.http.get<ApiResponse<RecommendedItem[]>>(`${this.base}/recommended`);
  }

  submit(request: SubmitRatingRequest): Observable<ApiResponse<RatingStats>> {
    return this.http.post<ApiResponse<RatingStats>>(this.base, request);
  }

  /** Session TTL: 4 hours. After this window the session resets so a new diner at the
   *  same table (same device) gets a clean slate and can rate independently. */
  private static readonly SESSION_TTL_MS = 4 * 60 * 60 * 1000;

  private static readonly KEY_SESSION   = 'fos_rating_session';
  private static readonly KEY_SESSION_TS = 'fos_rating_session_ts';
  private static readonly KEY_RATED     = 'fos_rated_items';

  /**
   * Returns the current session ID, creating or rotating it as needed.
   * A session is rotated (new ID + cleared rated list) when SESSION_TTL_MS has elapsed,
   * so a new diner at the same table starts fresh even on a shared/tablet device.
   */
  static getOrCreateSessionId(): string {
    const stored = localStorage.getItem(RatingService.KEY_SESSION);
    const ts     = parseInt(localStorage.getItem(RatingService.KEY_SESSION_TS) ?? '0', 10);
    const now    = Date.now();

    if (stored && (now - ts) < RatingService.SESSION_TTL_MS) {
      return stored;
    }

    // Session is new or has expired — rotate to a fresh one
    const id = 'ses_' + Math.random().toString(36).slice(2) + now.toString(36);
    localStorage.setItem(RatingService.KEY_SESSION,    id);
    localStorage.setItem(RatingService.KEY_SESSION_TS, String(now));
    localStorage.removeItem(RatingService.KEY_RATED);   // clear stale "rated" flags
    return id;
  }

  /** Returns the set of item IDs already rated in the current session. */
  static getRatedItems(): Set<number> {
    try {
      const raw = localStorage.getItem(RatingService.KEY_RATED);
      return raw ? new Set(JSON.parse(raw) as number[]) : new Set();
    } catch { return new Set(); }
  }

  static markRated(foodItemId: number): void {
    const items = RatingService.getRatedItems();
    items.add(foodItemId);
    localStorage.setItem(RatingService.KEY_RATED, JSON.stringify([...items]));
  }
}
