import { Injectable, signal, computed } from '@angular/core';

const CART_KEY = 'patient_cart';

export interface CartItem {
  medicineId: number;
  name: string;
  mrp: number;
  unit: string;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class PatientCartService {
  private items = signal<CartItem[]>(this.readFromStorage());

  readonly cartItems = this.items.asReadonly();
  readonly count = computed(() => this.items().reduce((sum, i) => sum + i.quantity, 0));
  readonly total = computed(() => this.items().reduce((sum, i) => sum + i.quantity * i.mrp, 0));

  private readFromStorage(): CartItem[] {
    try {
      const raw = localStorage.getItem(CART_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private persist() {
    localStorage.setItem(CART_KEY, JSON.stringify(this.items()));
  }

  add(item: Omit<CartItem, 'quantity'>, quantity = 1) {
    const existing = this.items().find(i => i.medicineId === item.medicineId);
    if (existing) {
      this.updateQuantity(item.medicineId, existing.quantity + quantity);
      return;
    }
    this.items.set([...this.items(), { ...item, quantity }]);
    this.persist();
  }

  updateQuantity(medicineId: number, quantity: number) {
    if (quantity <= 0) {
      this.remove(medicineId);
      return;
    }
    this.items.set(this.items().map(i => i.medicineId === medicineId ? { ...i, quantity } : i));
    this.persist();
  }

  remove(medicineId: number) {
    this.items.set(this.items().filter(i => i.medicineId !== medicineId));
    this.persist();
  }

  clear() {
    this.items.set([]);
    this.persist();
  }
}
