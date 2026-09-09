import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { toObservable } from '@angular/core/rxjs-interop';
import { switchMap, EMPTY } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../authentication/auth.service';
import { APP_CONSTANTS } from '../constants/app.constants';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class PermissionStateService {
  private http         = inject(HttpClient);
  private authService  = inject(AuthService);

  private _permissions = signal<Set<string>>(new Set());
  private _loaded      = signal(false);

  constructor() {
    toObservable(this.authService.currentUser)
      .pipe(
        switchMap(user => {
          if (!user) {
            this._permissions.set(new Set());
            this._loaded.set(false);
            return EMPTY;
          }
          return this.http.get<ApiResponse<string[]>>(
            `${environment.apiUrl}/permission/role/${user.roleId}`
          );
        })
      )
      .subscribe({
        next: res => {
          this._permissions.set(new Set(res?.data ?? []));
          this._loaded.set(true);
        },
        error: () => {
          this._loaded.set(true); // fail-open on error so app still works
        },
      });
  }

  /** True if the current user has the given permission.
   *  Admin is always granted. Non-loaded state is fail-open. */
  hasPermission(feature: string): boolean {
    if (this.authService.userRole() === APP_CONSTANTS.ROLES.ADMIN) return true;
    if (!this._loaded()) return true;
    return this._permissions().has(feature);
  }

  /** Expose the raw set for the access-control page */
  get permissions(): Set<string> { return this._permissions(); }
  get isLoaded(): boolean        { return this._loaded(); }
}
