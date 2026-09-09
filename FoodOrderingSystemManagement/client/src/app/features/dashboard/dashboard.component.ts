import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatRippleModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/authentication/auth.service';
import { OrderService } from '../../core/services/order.service';
import { SettingsService } from '../../core/services/settings.service';
import { PromotionService } from '../promotions/services/promotion.service';
import { OrganizationService } from '../../core/services/organization.service';
import { PromotionDto, PROMO_TYPE_META } from '../promotions/models/promotion.model';
import { License } from '../../core/models/organization.model';
import { environment } from '../../../environments/environment';

type ApiStatusType = 'checking' | 'online' | 'offline';

interface QuickAction {
  label: string;
  icon: string;
  route: string;
  color: string;
  animClass: string;
  image?: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatIconModule, MatButtonModule, MatRippleModule, MatProgressBarModule, MatDividerModule, MatTooltipModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private authService      = inject(AuthService);
  private orderService     = inject(OrderService);
  private promoService     = inject(PromotionService);
  private settingsService  = inject(SettingsService);
  private orgService       = inject(OrganizationService);
  private http             = inject(HttpClient);
  currentUser = this.authService.currentUser;

  license = signal<License | null>(null);

  activePromos   = signal<PromotionDto[]>([]);
  promoSlideIdx  = signal(0);
  readonly promoMeta = PROMO_TYPE_META;

  // ── API health ──────────────────────────────────────────────────────────
  private readonly healthUrl = environment.apiUrl.replace(/\/api\/?$/, '') + '/health';
  private healthInterval: ReturnType<typeof setInterval> | null = null;

  apiStatus      = signal<ApiStatusType>('checking');
  apiLastChecked = signal<Date | null>(null);

  checkApiHealth(): void {
    this.apiStatus.set('checking');
    this.http.get(this.healthUrl, { responseType: 'text', observe: 'response' })
      .subscribe({
        next:  res => {
          this.apiStatus.set(res.status < 400 ? 'online' : 'offline');
          this.apiLastChecked.set(new Date());
        },
        error: () => {
          this.apiStatus.set('offline');
          this.apiLastChecked.set(new Date());
        },
      });
  }

  salesData = signal<any>({ daily: 0, monthly: 0, totalOrders: 0, avgOrderValue: 0 });
  monthlySalesData = signal<{date: string, sales: number}[]>([]);
  topSellingItems = signal<{name: string, quantity: number, sales: number}[]>([]);
  orderStatusData = signal<{status: string, count: number, percentage: number, color: string}[]>([]);
  expandedCard = signal<string | null>(null);

  readonly quickActions: QuickAction[] = [
    { label: 'New Order',   icon: 'point_of_sale',    route: '/pos',        color: '#bf360c', animClass: 'anim-pulse',   image: 'assets/images/order.jpg' },
    { label: 'View Orders', icon: 'receipt_long',     route: '/orders',     color: '#1565c0', animClass: 'anim-scroll',  image: 'https://png.pngtree.com/png-vector/20230124/ourmid/pngtree-order-now-banner-for-promotion-of-your-business-png-image_6565761.png' },
    { label: 'Tables',      icon: 'table_restaurant', route: '/tables',     color: '#00695c', animClass: 'anim-wobble',  image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRT-1o2HK8ATleATvvqH5306Ht-LYCI7kuQUsX9mnnAwogjiyiJx8C4t2Tr&s=10' },
    { label: 'Waiter',      icon: 'room_service',     route: '/waiter',     color: '#4527a0', animClass: 'anim-swing',   image: 'https://img.magnific.com/free-vector/hand-drawn-waiter-cartoon-illustration_23-2150765978.jpg' },
    { label: 'Categories',  icon: 'category',         route: '/categories', color: '#2e7d32', animClass: 'anim-spin',    image: 'assets/images/categories.jpg' },
    { label: 'Food Items',  icon: 'fastfood',         route: '/food-items', color: '#6a1b9a', animClass: 'anim-bounce',  image: 'https://t3.ftcdn.net/jpg/08/12/90/84/360_F_812908455_T7QqaqGrFipIIBwe03aO4A4opZi12QAK.jpg' },
    { label: 'Settings',    icon: 'settings',         route: '/settings',   color: '#e65100', animClass: 'anim-gear',    image: 'assets/images/settings.jpg' },
  ];

  private promoInterval: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.checkApiHealth();
    this.healthInterval = setInterval(() => this.checkApiHealth(), 30_000);

    // Load settings first so promoAutoScroll flag is available, then load promos
    if (this.settingsService.settings()) {
      this.loadActivePromos();
    } else {
      this.settingsService.load().subscribe({ next: () => this.loadActivePromos(), error: () => this.loadActivePromos() });
    }

    if (this.isSuperAdmin()) {
      this.loadSalesData();
    } else {
      this.loadLicense();
    }
  }

  private loadLicense(): void {
    const orgId = this.authService.getOrganizationId();
    if (!orgId) return;
    this.orgService.getOrgLicense(orgId).subscribe({
      next: lic => this.license.set(lic),
      error: () => {},
    });
  }

  licenseStatus(): 'ok' | 'trial' | 'expiring' | 'expired' | null {
    const lic = this.license();
    if (!lic) return null;
    if (lic.status === 'Expired') return 'expired';
    const days = Math.ceil((new Date(lic.expiryDate).getTime() - Date.now()) / 86400000);
    if (days < 0)   return 'expired';
    if (days <= 7)  return 'expiring';
    if (lic.status === 'Trial') return 'trial';
    return 'ok';
  }

  licenseDaysLeft(): number {
    const lic = this.license();
    if (!lic) return 0;
    return Math.max(0, Math.ceil((new Date(lic.expiryDate).getTime() - Date.now()) / 86400000));
  }

  private loadActivePromos(): void {
    this.promoService.getActive().subscribe({
      next: list => {
        this.activePromos.set(list);
        const autoScroll = this.settingsService.settings()?.promoAutoScroll ?? true;
        if (list.length > 1 && autoScroll) {
          this.promoInterval = setInterval(() => {
            this.promoSlideIdx.update(i => (i + 1) % list.length);
          }, 3500);
        }
      },
    });
  }

  promoTypeColor(t: string): string {
    return this.promoMeta[t as keyof typeof this.promoMeta]?.color ?? '#9e9e9e';
  }
  promoTypeLabel(t: string): string {
    return this.promoMeta[t as keyof typeof this.promoMeta]?.label ?? t;
  }
  promoDiscountLabel(p: PromotionDto): string {
    if (p.discountType === 'Percentage') return `${p.discountValue}% OFF`;
    if (p.discountType === 'FlatAmount') return `₹${p.discountValue} OFF`;
    if (p.discountType === 'FreeItem')   return 'FREE ITEM';
    return `${p.discountValue}× Points`;
  }

  ngOnDestroy(): void {
    if (this.healthInterval) clearInterval(this.healthInterval);
    if (this.promoInterval)  clearInterval(this.promoInterval);
  }

  isSuperAdmin(): boolean {
    const roleName = this.currentUser()?.roleName ?? '';
    const lowerRole = roleName.toLowerCase().trim();
    return lowerRole === 'admin' || lowerRole === 'super admin';
  }

  private loadSalesData(): void {
    this.orderService.getAll().subscribe((res) => {
      if (res.success && res.data) {
        const orders = res.data;
        const today = new Date().toDateString();
        const currentMonth = new Date().getMonth();
        const currentYear = new Date().getFullYear();

        const todaysOrders = orders.filter((o) => new Date(o.createdDate).toDateString() === today);
        const monthlyOrders = orders.filter((o) => {
          const orderDate = new Date(o.createdDate);
          return orderDate.getMonth() === currentMonth && orderDate.getFullYear() === currentYear;
        });

        const dailySales = todaysOrders.reduce((sum, o) => sum + o.grandTotal, 0);
        const monthlySales = monthlyOrders.reduce((sum, o) => sum + o.grandTotal, 0);

        this.salesData.set({
          daily: dailySales,
          monthly: monthlySales,
          totalOrders: orders.length,
          avgOrderValue: orders.length ? (orders.reduce((sum, o) => sum + o.grandTotal, 0) / orders.length) : 0,
        });

        this.calculateMonthlySalesData(orders);
        this.calculateTopSellingItems(orders);
        this.calculateOrderStatuses(orders);
      }
    });
  }

  private calculateMonthlySalesData(orders: any[]): void {
    const monthlyData: {[key: string]: number} = {};
    orders.forEach((order) => {
      const date = new Date(order.createdDate);
      const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
      monthlyData[key] = (monthlyData[key] || 0) + order.grandTotal;
    });

    const data = Object.entries(monthlyData)
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([date, sales]) => ({date, sales: sales as number}))
      .slice(-12);

    this.monthlySalesData.set(data);
  }

  private calculateTopSellingItems(orders: any[]): void {
    const itemMap: {[key: string]: {name: string, quantity: number, sales: number}} = {};

    orders.forEach((order) => {
      order.items.forEach((item: any) => {
        if (!itemMap[item.itemName]) {
          itemMap[item.itemName] = {name: item.itemName, quantity: 0, sales: 0};
        }
        itemMap[item.itemName].quantity += item.quantity;
        itemMap[item.itemName].sales += item.lineTotal;
      });
    });

    const topItems = Object.values(itemMap)
      .sort((a, b) => b.sales - a.sales)
      .slice(0, 5);

    this.topSellingItems.set(topItems);
  }

  private calculateOrderStatuses(orders: any[]): void {
    const statusCounts: {[key: string]: number} = {
      'Pending': 0,
      'Completed': 0,
      'Cancelled': 0
    };

    orders.forEach((order) => {
      const status = order.status || 'Pending';
      if (status in statusCounts) {
        statusCounts[status]++;
      }
    });

    const total = orders.length || 1;
    const data = Object.entries(statusCounts).map(([status, count]) => {
      const colorMap: {[key: string]: string} = {
        'Completed': '#4caf50',
        'Pending': '#ff9800',
        'Cancelled': '#f44336'
      };
      return {
        status,
        count: count as number,
        percentage: Math.round(((count as number) / total) * 100),
        color: colorMap[status] || '#999'
      };
    });

    this.orderStatusData.set(data);
  }

  getGradientStops(): string {
    const data = this.orderStatusData();
    let currentPercent = 0;
    const stops: string[] = [];

    data.forEach((item) => {
      const nextPercent = currentPercent + item.percentage;
      stops.push(`${item.color} ${currentPercent}%, ${item.color} ${nextPercent}%`);
      currentPercent = nextPercent;
    });

    return `conic-gradient(${stops.join(', ')})`;
  }

  toggleCard(cardId: string): void {
    this.expandedCard.set(this.expandedCard() === cardId ? null : cardId);
  }

  isCardExpanded(cardId: string): boolean {
    return this.expandedCard() === cardId;
  }

  getCardDetails(cardId: string): {label: string, value: string, icon: string, color: string}[] {
    const today = new Date().toDateString();
    const currentMonth = new Date().getMonth();
    const currentYear = new Date().getFullYear();

    const orders = this.orderStatusData().reduce((sum, s) => sum + s.count, 0);

    switch (cardId) {
      case 'daily':
        return [
          {label: 'Orders Today', value: String(orders), icon: 'shopping_cart', color: '#d32f2f'},
          {label: 'Avg Per Order', value: `₹${(this.salesData().daily / (orders || 1)).toFixed(2)}`, icon: 'trending_up', color: '#1976d2'},
          {label: 'Highest Sale', value: '₹5,000', icon: 'star', color: '#f57c00'},
        ];
      case 'monthly':
        return [
          {label: 'Orders This Month', value: String(orders), icon: 'calendar_month', color: '#1976d2'},
          {label: 'Avg Per Order', value: `₹${(this.salesData().monthly / (orders || 1)).toFixed(2)}`, icon: 'trending_up', color: '#388e3c'},
          {label: 'Growth vs Last Month', value: '+12%', icon: 'arrow_upward', color: '#388e3c'},
        ];
      case 'orders':
        return [
          {label: 'Completed', value: String(this.orderStatusData().find(s => s.status === 'Completed')?.count || 0), icon: 'check_circle', color: '#4caf50'},
          {label: 'Pending', value: String(this.orderStatusData().find(s => s.status === 'Pending')?.count || 0), icon: 'hourglass_empty', color: '#ff9800'},
          {label: 'Cancelled', value: String(this.orderStatusData().find(s => s.status === 'Cancelled')?.count || 0), icon: 'cancel', color: '#f44336'},
        ];
      case 'average':
        return [
          {label: 'Highest Order', value: '₹5,000', icon: 'trending_up', color: '#f57c00'},
          {label: 'Lowest Order', value: '₹500', icon: 'trending_down', color: '#d32f2f'},
          {label: 'Total Revenue', value: `₹${this.salesData().monthly.toFixed(2)}`, icon: 'payments', color: '#1976d2'},
        ];
      default:
        return [];
    }
  }
}
