import {
  Component, OnInit, OnDestroy, inject, signal, computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject, takeUntil } from 'rxjs';
import { PromotionService } from './services/promotion.service';
import {
  PromotionDto, CreatePromotionDto, PromotionType,
  PROMO_TYPE_META,
} from './models/promotion.model';

const BANNER_COLORS = [
  '#e53935','#7b1fa2','#1565c0','#00695c',
  '#f57c00','#ad1457','#ff6f00','#2e7d32',
];

const DEFAULT_FORM: CreatePromotionDto = {
  name: '', description: '', promotionType: 'Coupon', discountType: 'Percentage',
  discountValue: 10, code: '', minOrderValue: undefined, maxDiscount: undefined,
  usageLimit: undefined, usageLimitPerCustomer: undefined,
  startDate: undefined, endDate: undefined,
  happyHourStart: '', happyHourEnd: '', happyHourDays: '',
  applicableItemIds: '', buyQty: undefined, getQty: undefined,
  giftCardBalance: undefined, spendThreshold: undefined, pointsMultiplierVal: undefined,
  isActive: true, isPublic: true, bannerColor: '#e53935', badgeIcon: '🎫',
};

@Component({
  selector: 'app-promotions',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule],
  templateUrl: './promotions.component.html',
  styleUrl: './promotions.component.scss',
})
export class PromotionsComponent implements OnInit, OnDestroy {
  private svc     = inject(PromotionService);
  private route   = inject(ActivatedRoute);
  private snack   = inject(MatSnackBar);
  private destroy = new Subject<void>();

  loading = signal(true);
  saving  = signal(false);
  promos  = signal<PromotionDto[]>([]);

  filterType   = signal<string>('all');
  filterStatus = signal<string>('all');
  search       = signal('');

  showPanel  = signal(false);
  editId     = signal<number | null>(null);
  form       = signal<CreatePromotionDto>({ ...DEFAULT_FORM });

  readonly promoTypes = Object.keys(PROMO_TYPE_META) as PromotionType[];
  readonly typeMeta   = PROMO_TYPE_META;
  readonly bannerColors = BANNER_COLORS;

  filteredPromos = computed(() => {
    let list = this.promos();
    const t = this.filterType();
    const s = this.filterStatus();
    const q = this.search().toLowerCase();
    if (t !== 'all') list = list.filter(p => p.promotionType === t);
    if (s !== 'all') list = list.filter(p => p.status === s);
    if (q) list = list.filter(p =>
      p.name.toLowerCase().includes(q) || (p.code?.toLowerCase().includes(q) ?? false)
    );
    return list;
  });

  counts = computed(() => ({
    all:      this.promos().length,
    active:   this.promos().filter(p => p.status === 'Active').length,
    expired:  this.promos().filter(p => p.status === 'Expired').length,
    upcoming: this.promos().filter(p => p.status === 'Upcoming').length,
  }));

  ngOnInit(): void {
    // Apply ?type= query param from sidebar navigation (Coupons, Happy Hours, Gift Cards)
    this.route.queryParams.pipe(takeUntil(this.destroy)).subscribe(params => {
      const type = params['type'];
      if (type && Object.keys(PROMO_TYPE_META).includes(type)) {
        this.filterType.set(type);
      } else {
        this.filterType.set('all');
      }
    });
    this.load();
  }

  ngOnDestroy(): void { this.destroy.next(); this.destroy.complete(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll().pipe(takeUntil(this.destroy)).subscribe({
      next: list => { this.promos.set(list); this.loading.set(false); },
      error: err => {
        this.loading.set(false);
        this.snack.open('Failed to load promotions. Please try again.', 'Close', { duration: 4000, panelClass: 'snack-error' });
      },
    });
  }

  openCreate(): void {
    this.editId.set(null);
    this.form.set({ ...DEFAULT_FORM, badgeIcon: this.typeMeta['Coupon'].defaultBadge });
    this.showPanel.set(true);
  }

  openEdit(p: PromotionDto): void {
    this.editId.set(p.id);
    this.form.set({
      name: p.name, description: p.description ?? '',
      promotionType: p.promotionType, discountType: p.discountType,
      discountValue: p.discountValue, code: p.code ?? '',
      minOrderValue: p.minOrderValue, maxDiscount: p.maxDiscount,
      usageLimit: p.usageLimit, usageLimitPerCustomer: p.usageLimitPerCustomer,
      startDate: p.startDate ? p.startDate.slice(0,10) as any : undefined,
      endDate:   p.endDate   ? p.endDate.slice(0,10) as any : undefined,
      happyHourStart: p.happyHourStart ?? '', happyHourEnd: p.happyHourEnd ?? '',
      happyHourDays: p.happyHourDays ?? '',
      applicableItemIds: p.applicableItemIds ?? '',
      buyQty: p.buyQty, getQty: p.getQty,
      giftCardBalance: p.giftCardBalance, spendThreshold: p.spendThreshold,
      pointsMultiplierVal: p.pointsMultiplierVal,
      isActive: p.isActive, isPublic: p.isPublic,
      bannerColor: p.bannerColor ?? '#e53935', badgeIcon: p.badgeIcon ?? '🎫',
    });
    this.showPanel.set(true);
  }

  closePanel(): void { this.showPanel.set(false); }

  onTypeChange(): void {
    const t = this.form().promotionType;
    const meta = this.typeMeta[t];
    this.form.update(f => ({
      ...f,
      badgeIcon: meta.defaultBadge,
      bannerColor: meta.color,
    }));
  }

  save(): void {
    const f = this.form();
    if (!f.name.trim()) return;
    this.saving.set(true);
    const isEdit = !!this.editId();
    const obs = isEdit
      ? this.svc.update(this.editId()!, f)
      : this.svc.create(f);
    obs.pipe(takeUntil(this.destroy)).subscribe({
      next: saved => {
        if (isEdit) {
          this.promos.update(list => list.map(p => p.id === saved.id ? saved : p));
        } else {
          this.promos.update(list => [saved, ...list]);
        }
        this.saving.set(false);
        this.showPanel.set(false);
        this.snack.open(isEdit ? 'Promotion updated.' : 'Promotion created.', 'Close', { duration: 3000, panelClass: 'snack-success' });
      },
      error: err => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Failed to save promotion. Please try again.';
        this.snack.open(msg, 'Close', { duration: 5000, panelClass: 'snack-error' });
      },
    });
  }

  toggle(p: PromotionDto): void {
    this.svc.toggle(p.id).pipe(takeUntil(this.destroy)).subscribe({
      next: res => {
        this.promos.update(list =>
          list.map(x => x.id === p.id ? { ...x, isActive: res.isActive, status: res.isActive ? 'Active' : 'Inactive' } : x)
        );
        this.snack.open(res.isActive ? `"${p.name}" activated.` : `"${p.name}" deactivated.`, 'Close', { duration: 2500 });
      },
      error: () => this.snack.open('Failed to toggle promotion.', 'Close', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  removePromo(p: PromotionDto): void {
    if (!confirm(`Delete promotion "${p.name}"?`)) return;
    this.svc.delete(p.id).pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.promos.update(list => list.filter(x => x.id !== p.id));
        this.snack.open(`"${p.name}" deleted.`, 'Close', { duration: 2500 });
      },
      error: () => this.snack.open('Failed to delete promotion.', 'Close', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  typeLabel(t: string): string  { return this.typeMeta[t as PromotionType]?.label ?? t; }
  typeIcon(t: string): string   { return this.typeMeta[t as PromotionType]?.icon ?? 'local_offer'; }
  typeColor(t: string): string  { return this.typeMeta[t as PromotionType]?.color ?? '#9e9e9e'; }

  discountLabel(p: PromotionDto): string {
    if (p.discountType === 'Percentage')   return `${p.discountValue}% off`;
    if (p.discountType === 'FlatAmount')   return `₹${p.discountValue} off`;
    if (p.discountType === 'FreeItem')     return 'Free item';
    if (p.discountType === 'PointsMultiplier') return `${p.discountValue}× points`;
    return '';
  }

  patchForm(partial: Partial<CreatePromotionDto>): void {
    this.form.update(f => ({ ...f, ...partial }));
  }

  showCodeField = computed(() => {
    const t = this.form().promotionType;
    return ['Coupon','PromoCode','GiftCard'].includes(t);
  });
  showHappyHour = computed(() => this.form().promotionType === 'HappyHour');
  showBogo      = computed(() => this.form().promotionType === 'BuyOneGetOne' || this.form().promotionType === 'ComboOffer');
  showGiftCard  = computed(() => this.form().promotionType === 'GiftCard');
  showLoyalty   = computed(() => this.form().promotionType === 'LoyaltyReward');
  showDateRange = computed(() => !['HappyHour','LoyaltyReward'].includes(this.form().promotionType));
}
