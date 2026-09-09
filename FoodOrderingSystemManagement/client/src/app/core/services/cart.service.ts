import { Injectable, computed, signal } from '@angular/core';
import { CartItem } from '../models/order.model';
import { SettingsService } from './settings.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private _items = signal<CartItem[]>([]);

  readonly items     = this._items.asReadonly();
  readonly itemCount = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));
  readonly isEmpty   = computed(() => this._items().length === 0);

  readonly subTotal = computed(() =>
    this._items().reduce((sum, i) => sum + (i.price + i.addOnTotal) * i.quantity, 0)
  );

  readonly tax = computed(() => {
    const pct = this.settingsService.settings()?.taxPercentage ?? 0;
    return parseFloat(((this.subTotal() * pct) / 100).toFixed(2));
  });

  readonly grandTotal = computed(() =>
    parseFloat((this.subTotal() + this.tax() - this.discount()).toFixed(2))
  );

  private _discount = signal(0);
  readonly discount  = this._discount.asReadonly();

  constructor(private settingsService: SettingsService) {}

  add(item: CartItem): void {
    this._items.update((list) => {
      const idx = list.findIndex((i) => i.foodItemId === item.foodItemId);
      if (idx >= 0) {
        const updated = [...list];
        updated[idx] = { ...updated[idx], quantity: updated[idx].quantity + 1 };
        return updated;
      }
      return [...list, { ...item, quantity: 1 }];
    });
  }

  increase(foodItemId: number): void {
    this._items.update((list) =>
      list.map((i) => i.foodItemId === foodItemId ? { ...i, quantity: i.quantity + 1 } : i)
    );
  }

  decrease(foodItemId: number): void {
    this._items.update((list) => {
      const item = list.find((i) => i.foodItemId === foodItemId);
      if (!item) return list;
      if (item.quantity <= 1) return list.filter((i) => i.foodItemId !== foodItemId);
      return list.map((i) => i.foodItemId === foodItemId ? { ...i, quantity: i.quantity - 1 } : i);
    });
  }

  remove(foodItemId: number): void {
    this._items.update((list) => list.filter((i) => i.foodItemId !== foodItemId));
  }

  setDiscount(value: number): void {
    this._discount.set(value < 0 ? 0 : value);
  }

  clear(): void {
    this._items.set([]);
    this._discount.set(0);
  }

  isInCart(foodItemId: number): boolean {
    return this._items().some((i) => i.foodItemId === foodItemId);
  }

  quantityOf(foodItemId: number): number {
    return this._items().find((i) => i.foodItemId === foodItemId)?.quantity ?? 0;
  }
}
