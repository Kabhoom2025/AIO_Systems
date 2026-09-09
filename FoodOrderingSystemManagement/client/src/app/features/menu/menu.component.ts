import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddOn, CartItemWithAddOns } from '../../core/models/addon.model';
import { RatingStats, RecommendedItem } from '../../core/models/rating.model';
import { RatingService } from '../../core/services/rating.service';

interface MenuItem {
  id: number;
  itemName: string;
  price: number;
  image: string | null;
  description: string | null;
  isAvailable: boolean;
  categoryName?: string;
}

interface MenuCategory {
  id: number;
  categoryName: string;
  items: MenuItem[];
}

interface MenuData {
  restaurantName: string;
  categories: MenuCategory[];
}

// Contextual add-ons per food type
const ADDONS: Record<string, AddOn[]> = {
  pizza:    [
    { id: 1, name: 'Extra Cheese',   price: 50, category: 'cheese'  },
    { id: 2, name: 'Pepperoni',      price: 60, category: 'topping' },
    { id: 3, name: 'Mushrooms',      price: 40, category: 'topping' },
    { id: 4, name: 'Bell Peppers',   price: 35, category: 'topping' },
    { id: 5, name: 'Olives',         price: 45, category: 'topping' },
    { id: 6, name: 'Garlic Sauce',   price: 25, category: 'sauce'   },
  ],
  burger:   [
    { id: 10, name: 'Extra Patty',   price: 80, category: 'protein' },
    { id: 11, name: 'Cheese Slice',  price: 40, category: 'cheese'  },
    { id: 12, name: 'Jalapeños',     price: 20, category: 'topping' },
    { id: 13, name: 'BBQ Sauce',     price: 25, category: 'sauce'   },
    { id: 14, name: 'Caramelised Onion', price: 20, category: 'topping' },
  ],
  biryani:  [
    { id: 20, name: 'Extra Raita',   price: 30, category: 'side'    },
    { id: 21, name: 'Extra Salan',   price: 25, category: 'side'    },
    { id: 22, name: 'Boiled Egg',    price: 20, category: 'protein' },
    { id: 23, name: 'Extra Rice',    price: 40, category: 'carb'    },
  ],
  chicken:  [
    { id: 30, name: 'Extra Piece',   price: 70, category: 'protein' },
    { id: 31, name: 'Make It Spicy', price: 10, category: 'spice'   },
    { id: 32, name: 'BBQ Sauce',     price: 20, category: 'sauce'   },
    { id: 33, name: 'Coleslaw',      price: 30, category: 'side'    },
  ],
  coffee:   [
    { id: 40, name: 'Extra Shot',    price: 30, category: 'coffee'  },
    { id: 41, name: 'Oat Milk',      price: 40, category: 'milk'    },
    { id: 42, name: 'Caramel Drizzle', price: 25, category: 'sweet' },
    { id: 43, name: 'Whipped Cream', price: 35, category: 'topping' },
  ],
  dessert:  [
    { id: 50, name: 'Extra Scoop',   price: 60, category: 'portion' },
    { id: 51, name: 'Chocolate Sauce', price: 30, category: 'sauce' },
    { id: 52, name: 'Crushed Nuts',  price: 25, category: 'topping' },
    { id: 53, name: 'Wafer Cone',    price: 20, category: 'extra'   },
  ],
  default:  [
    { id: 60, name: 'Extra Serving', price: 50, category: 'portion' },
    { id: 61, name: 'Make It Spicy', price: 10, category: 'spice'   },
    { id: 62, name: 'Less Spicy',    price:  0, category: 'spice'   },
    { id: 63, name: 'No Onion',      price:  0, category: 'custom'  },
  ],
};

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.scss',
})
export class MenuComponent implements OnInit {
  private http          = inject(HttpClient);
  private route         = inject(ActivatedRoute);
  private ratingService = inject(RatingService);

  tableNumber      = signal<string | null>(null);
  loading          = signal(true);
  error            = signal<string | null>(null);
  menu             = signal<MenuData | null>(null);
  activeCategory   = signal<number | null>(null);
  showCart         = signal(false);

  // Rating state
  ratingStats       = signal<Map<number, RatingStats>>(new Map());
  recommended       = signal<RecommendedItem[]>([]);
  ratedItems        = signal<Set<number>>(new Set());

  // Rating modal
  ratingItem        = signal<MenuItem | null>(null);
  hoverStar         = signal(0);
  selectedStar      = signal(0);
  ratingComment     = signal('');
  ratingSubmitting  = signal(false);
  ratingError       = signal('');
  ratingDone        = signal(false);

  // Add-ons modal state
  selectedItemForAddOns = signal<MenuItem | null>(null);
  selectedAddOns        = signal<AddOn[]>([]);

  // Cart state
  cart = signal<CartItemWithAddOns[]>([]);

  cartTotal = computed(() =>
    this.cart().reduce((sum, e) => sum + e.totalPrice * e.quantity, 0)
  );
  cartItemCount = computed(() =>
    this.cart().reduce((sum, e) => sum + e.quantity, 0)
  );

  // Checkout state
  showCheckout      = signal(false);
  checkoutName      = signal('');
  checkoutPhone     = signal('');
  checkoutAddress   = signal('');
  checkoutNotes     = signal('');
  checkoutSubmitting = signal(false);
  checkoutError     = signal('');
  confirmedOrder    = signal<{ orderNumber: string; grandTotal: number; estimatedTime: string | null } | null>(null);

  cartQtyOf(itemId: number): number {
    return this.cart()
      .filter(e => e.itemId === itemId)
      .reduce((s, e) => s + e.quantity, 0);
  }

  currentAddOns = computed<AddOn[]>(() => {
    const item = this.selectedItemForAddOns();
    if (!item) return [];
    return this.getAddOnsForItem(item);
  });

  visibleItems = computed(() => {
    const m   = this.menu();
    const cat = this.activeCategory();
    if (!m) return [];
    const items = cat === null
      ? m.categories.flatMap(c => c.items.map(i => ({ ...i, categoryName: c.categoryName })))
      : (m.categories.find(c => c.id === cat)?.items ?? []).map(i => ({
          ...i,
          categoryName: m.categories.find(c => c.id === cat)?.categoryName ?? ''
        }));
    return items;
  });

  ngOnInit(): void {
    this.tableNumber.set(this.route.snapshot.queryParamMap.get('table'));
    // Calling getOrCreateSessionId() first ensures an expired session is rotated
    // (and its stale rated-items list cleared) before we read it.
    RatingService.getOrCreateSessionId();
    this.ratedItems.set(RatingService.getRatedItems());

    forkJoin({
      menu:  this.http.get<any>(`${environment.apiUrl}/menu`),
      stats: this.ratingService.getAllStats().pipe(catchError(() => of({ data: [] }))),
      recommended: this.ratingService.getRecommended().pipe(catchError(() => of({ data: [] }))),
    }).subscribe({
      next: ({ menu, stats, recommended }) => {
        this.menu.set(menu.data);
        const first = menu.data?.categories?.[0];
        if (first) this.activeCategory.set(first.id);

        const map = new Map<number, RatingStats>();
        (stats.data ?? []).forEach((s: RatingStats) => map.set(s.foodItemId, s));
        this.ratingStats.set(map);

        this.recommended.set((recommended.data ?? []) as RecommendedItem[]);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load menu. Please try again.');
        this.loading.set(false);
      },
    });
  }

  // ── Rating helpers ───────────────────────────────────────────────────────
  getStats(itemId: number): RatingStats | undefined {
    return this.ratingStats().get(itemId);
  }

  getAvgRating(itemId: number): number {
    return this.getStats(itemId)?.averageRating ?? 0;
  }

  getRatingCount(itemId: number): number {
    return this.getStats(itemId)?.ratingCount ?? 0;
  }

  hasRated(itemId: number): boolean {
    return this.ratedItems().has(itemId);
  }

  /** Returns array [1..5] used by @for to render stars */
  readonly starRange = [1, 2, 3, 4, 5];

  starFill(itemId: number, star: number): 'full' | 'half' | 'empty' {
    return this.avgStarFill(this.getAvgRating(itemId), star);
  }

  avgStarFill(avg: number, star: number): 'full' | 'half' | 'empty' {
    if (star <= Math.floor(avg)) return 'full';
    if (star === Math.ceil(avg) && avg % 1 >= 0.3) return 'half';
    return 'empty';
  }

  openRatingModal(item: MenuItem, event: Event): void {
    event.stopPropagation();
    this.ratingItem.set(item);
    this.selectedStar.set(0);
    this.hoverStar.set(0);
    this.ratingComment.set('');
    this.ratingError.set('');
    this.ratingDone.set(false);
  }

  closeRatingModal(): void {
    this.ratingItem.set(null);
    this.ratingDone.set(false);
  }

  submitRating(): void {
    const item = this.ratingItem();
    const star = this.selectedStar();
    if (!item || star < 1) { this.ratingError.set('Please select a star rating.'); return; }

    this.ratingSubmitting.set(true);
    this.ratingError.set('');

    const sessionId = RatingService.getOrCreateSessionId();
    this.ratingService.submit({
      foodItemId:  item.id,
      rating:      star,
      comment:     this.ratingComment() || undefined,
      sessionId,
      tableNumber: this.tableNumber() ?? undefined,
    }).subscribe({
      next: (res) => {
        // Update local stats immediately
        const map = new Map(this.ratingStats());
        map.set(item.id, res.data);
        this.ratingStats.set(map);

        RatingService.markRated(item.id);
        this.ratedItems.set(RatingService.getRatedItems());

        this.ratingSubmitting.set(false);
        this.ratingDone.set(true);
      },
      error: (err) => {
        this.ratingSubmitting.set(false);
        this.ratingError.set(err?.error?.message ?? 'Failed to submit rating. Please try again.');
      },
    });
  }

  // ── Category icon ────────────────────────────────────────────────────────
  getCategoryIcon(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('pizza'))    return '🍕';
    if (n.includes('burger'))   return '🍔';
    if (n.includes('biryani'))  return '🍛';
    if (n.includes('drink') || n.includes('beverage')) return '🥤';
    if (n.includes('ice'))      return '🍦';
    if (n.includes('dessert'))  return '🍰';
    if (n.includes('chinese'))  return '🍜';
    if (n.includes('sandwich')) return '🥪';
    if (n.includes('chicken'))  return '🍗';
    if (n.includes('coffee'))   return '☕';
    if (n.includes('soup'))     return '🍲';
    if (n.includes('salad'))    return '🥗';
    return '🍽️';
  }

  // ── Add-ons modal ────────────────────────────────────────────────────────
  getAddOnsForItem(item: MenuItem): AddOn[] {
    const n = (item.categoryName ?? item.itemName).toLowerCase();
    if (n.includes('pizza'))                          return ADDONS['pizza'];
    if (n.includes('burger'))                         return ADDONS['burger'];
    if (n.includes('biryani') || n.includes('rice'))  return ADDONS['biryani'];
    if (n.includes('chicken') || n.includes('grill')) return ADDONS['chicken'];
    if (n.includes('coffee') || n.includes('latte') || n.includes('cappuccino')) return ADDONS['coffee'];
    if (n.includes('dessert') || n.includes('ice') || n.includes('cake')) return ADDONS['dessert'];
    return ADDONS['default'];
  }

  openAddOnsModal(item: MenuItem): void {
    this.selectedItemForAddOns.set(item);
    this.selectedAddOns.set([]);
  }

  closeAddOnsModal(): void {
    this.selectedItemForAddOns.set(null);
    this.selectedAddOns.set([]);
  }

  toggleAddOn(addOn: AddOn): void {
    const current = this.selectedAddOns();
    const exists  = current.find(a => a.id === addOn.id);
    this.selectedAddOns.set(
      exists ? current.filter(a => a.id !== addOn.id) : [...current, addOn]
    );
  }

  isAddOnSelected(addOnId: number): boolean {
    return this.selectedAddOns().some(a => a.id === addOnId);
  }

  getTotalAddOnsPrice(): number {
    return this.selectedAddOns().reduce((sum, a) => sum + a.price, 0);
  }

  // ── Cart actions ─────────────────────────────────────────────────────────
  addToCart(): void {
    const item = this.selectedItemForAddOns();
    if (!item) return;

    const addOns     = this.selectedAddOns();
    const totalPrice = item.price + this.getTotalAddOnsPrice();

    const existing = this.cart().find(e =>
      e.itemId === item.id &&
      e.selectedAddOns.length === addOns.length &&
      e.selectedAddOns.every(a => addOns.some(b => b.id === a.id))
    );

    if (existing) {
      this.cart.update(c =>
        c.map(e => e === existing ? { ...e, quantity: e.quantity + 1 } : e)
      );
    } else {
      const entry: CartItemWithAddOns = {
        itemId:         item.id,
        itemName:       item.itemName,
        quantity:       1,
        basePrice:      item.price,
        selectedAddOns: [...addOns],
        totalPrice,
      };
      this.cart.update(c => [...c, entry]);
    }

    this.closeAddOnsModal();
  }

  changeQty(entry: CartItemWithAddOns, delta: number): void {
    const newQty = entry.quantity + delta;
    if (newQty <= 0) {
      this.cart.update(c => c.filter(e => e !== entry));
    } else {
      this.cart.update(c =>
        c.map(e => e === entry ? { ...e, quantity: newQty } : e)
      );
    }
  }

  clearCart(): void {
    this.cart.set([]);
    this.showCart.set(false);
  }

  openCheckout(): void {
    this.showCart.set(false);
    this.checkoutError.set('');
    this.showCheckout.set(true);
  }

  closeCheckout(): void {
    this.showCheckout.set(false);
    this.checkoutError.set('');
  }

  placeOrder(): void {
    const isDelivery = !this.tableNumber();

    if (!this.checkoutPhone().trim()) {
      this.checkoutError.set('Phone number is required.');
      return;
    }
    if (isDelivery && !this.checkoutAddress().trim()) {
      this.checkoutError.set('Delivery address is required.');
      return;
    }

    this.checkoutSubmitting.set(true);
    this.checkoutError.set('');

    const payload = {
      customerName:    this.checkoutName().trim() || 'Guest',
      customerPhone:   this.checkoutPhone().trim(),
      deliveryAddress: isDelivery ? this.checkoutAddress().trim() : undefined,
      notes:           this.checkoutNotes().trim() || undefined,
      tableId:         this.tableNumber() ? parseInt(this.tableNumber()!) : undefined,
      orderType:       isDelivery ? 'Delivery' : 'DineIn',
      deliveryCharge:  0,
      items:           this.cart().map(e => ({
        foodItemId: e.itemId,
        quantity:   e.quantity,
        addOnIds:   e.selectedAddOns.map(a => a.id),
      })),
    };

    this.http.post<any>(`${environment.apiUrl}/public/orders`, payload).subscribe({
      next: res => {
        const d = res.data;
        this.confirmedOrder.set({
          orderNumber:   d.orderNumber,
          grandTotal:    d.grandTotal,
          estimatedTime: d.estimatedTime,
        });
        this.cart.set([]);
        this.showCheckout.set(false);
        this.checkoutSubmitting.set(false);
        this.checkoutName.set('');
        this.checkoutPhone.set('');
        this.checkoutAddress.set('');
        this.checkoutNotes.set('');
      },
      error: err => {
        this.checkoutError.set(err?.error?.message ?? 'Failed to place order. Please try again.');
        this.checkoutSubmitting.set(false);
      },
    });
  }

  dismissConfirmation(): void {
    this.confirmedOrder.set(null);
  }

  addonNames(addOns: AddOn[]): string {
    return addOns.map(a => a.name).join(', ');
  }
}
