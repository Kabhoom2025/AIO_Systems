import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';

interface ApiHealth {
  service: string;
  status: string;
  timeUtc: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private readonly http = inject(HttpClient);

  readonly apiHealth = signal<ApiHealth | null>(null);
  readonly apiUnreachable = signal(false);
  readonly checking = signal(true);

  constructor() {
    this.http
      .get<ApiHealth>(`${environment.apiBaseUrl}/`)
      .pipe(
        catchError(() => {
          this.apiUnreachable.set(true);
          return of(null);
        })
      )
      .subscribe((health) => {
        this.apiHealth.set(health);
        this.checking.set(false);
      });
  }
}
