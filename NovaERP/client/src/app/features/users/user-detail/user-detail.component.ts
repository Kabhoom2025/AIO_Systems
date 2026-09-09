import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { NotificationService } from '../../../core/notification.service';
import { UserApiService, UserDto, UserSalesSummaryDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, TagModule, ToastModule],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.scss'
})
export class UserDetailComponent implements OnInit {
  user: UserDto | null = null;
  summary: UserSalesSummaryDto | null = null;
  loading = false;
  salesUnavailable = false;

  private readonly statusPalette: Record<string, string> = {
    Draft: '#8d897f',
    Confirmed: '#5f8a4c',
    Cancelled: '#c0574f'
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userApi: UserApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;
    this.load(id);
  }

  private load(id: number) {
    this.loading = true;
    this.userApi.getById(id).subscribe({
      next: user => {
        this.user = user;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load user.');
      }
    });

    this.userApi.getSalesSummary(id).subscribe({
      next: summary => (this.summary = summary),
      error: () => (this.salesUnavailable = true)
    });
  }

  back() {
    this.router.navigate(['/users']);
  }

  initials(name: string | undefined): string {
    if (!name) return '?';
    return name.trim().charAt(0).toUpperCase();
  }

  statusColor(status: string): string {
    return this.statusPalette[status] ?? '#8d897f';
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Confirmed') return 'success';
    if (status === 'Cancelled') return 'danger';
    return 'info';
  }

  /// Donut arc geometry for a 3-way Draft/Confirmed/Cancelled split — circumference-based
  /// stroke-dasharray/offset, same approach used for the Rate Quotes carrier donut.
  donutSegments(): { color: string; dasharray: string; dashoffset: string }[] {
    if (!this.summary || this.summary.totalOrders === 0) return [];
    const r = 70;
    const circumference = 2 * Math.PI * r;
    const parts = [
      { count: this.summary.draftOrders, color: this.statusPalette['Draft'] },
      { count: this.summary.confirmedOrders, color: this.statusPalette['Confirmed'] },
      { count: this.summary.cancelledOrders, color: this.statusPalette['Cancelled'] }
    ];
    let offsetSoFar = 0;
    return parts
      .filter(p => p.count > 0)
      .map(p => {
        const length = (p.count / this.summary!.totalOrders) * circumference;
        const seg = { color: p.color, dasharray: `${length} ${circumference}`, dashoffset: `${-offsetSoFar}` };
        offsetSoFar += length;
        return seg;
      });
  }

  pct(count: number): number {
    if (!this.summary || this.summary.totalOrders === 0) return 0;
    return Math.round((count / this.summary.totalOrders) * 100);
  }
}
