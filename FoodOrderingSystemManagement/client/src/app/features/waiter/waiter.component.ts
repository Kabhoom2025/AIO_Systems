import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRippleModule } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { FoodItemService } from '../../core/services/food-item.service';
import { CategoryService } from '../../core/services/category.service';
import { TableService } from '../../core/services/table.service';
import { OrderService } from '../../core/services/order.service';
import { AddOnService } from '../../core/services/addon.service';
import { NotificationService } from '../../shared/services/notification.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { FoodItem } from '../../core/models/food-item.model';
import { Category } from '../../core/models/category.model';
import { Table, HallType } from '../../core/models/table.model';
import { Order, CartItem } from '../../core/models/order.model';
import { AddOn } from '../../core/models/addon.model';
import { AddOnPickerDialogComponent, AddOnPickerResult } from '../orders/pos/addon-picker-dialog.component';
import { SignalRService, OrderStatusChangedEvent } from '../../core/services/signalr.service';
import { Subscription } from 'rxjs';

interface KitchenItem { name: string; qty: number; }

@Component({
  selector: 'app-waiter',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule, MatRippleModule, LoadingSpinnerComponent],
  templateUrl: './waiter.component.html',
  styleUrl: './waiter.component.scss',
})
export class WaiterComponent implements OnInit, OnDestroy {
  private foodItemService = inject(FoodItemService);
  private categoryService = inject(CategoryService);
  private tableService    = inject(TableService);
  private orderService    = inject(OrderService);
  private addOnService    = inject(AddOnService);
  private dialog          = inject(MatDialog);
  private notify          = inject(NotificationService);

  foodItems      = signal<FoodItem[]>([]);
  categories     = signal<Category[]>([]);
  tables         = signal<Table[]>([]);
  pendingOrders  = signal<Order[]>([]);
  allAddOns      = signal<AddOn[]>([]);
  loading        = signal(true);
  sending        = signal(false);
  generatingBill = signal(false);

  selectedTableId  = signal<number | null>(null);
  selectedCategory = signal('All');
  searchTerm       = signal('');
  cart             = signal<CartItem[]>([]);

  // Notification state
  notification       = signal<string | null>(null);
  notifOrderId       = signal<number | null>(null);
  notifPickingUp     = signal(false);
  notifTimeout: ReturnType<typeof setTimeout> | null = null;

  // Bill split state
  showSplitModal     = signal(false);
  splitCount         = signal(2);
  splitPendingOrder  = signal<import('../../core/models/order.model').Order | null>(null);

  private signalR    = inject(SignalRService);
  private signalRSub: Subscription | null = null;

  activeTableOrder = computed(() => {
    const id = this.selectedTableId();
    if (!id) return null;
    return this.pendingOrders().find(o => o.tableId === id) ?? null;
  });

  selectedTable = computed(() =>
    this.tables().find(t => t.id === this.selectedTableId()) ?? null
  );

  hallGroups = computed<{ hall: HallType; tables: Table[] }[]>(() => {
    const map = new Map<HallType, Table[]>();
    for (const t of this.tables()) {
      const h = (t.hall ?? 'AC') as HallType;
      if (!map.has(h)) map.set(h, []);
      map.get(h)!.push(t);
    }
    // Always show AC first, then Non-AC
    const order: HallType[] = ['AC', 'Non-AC'];
    return order.filter(h => map.has(h)).map(h => ({ hall: h, tables: map.get(h)! }));
  });

  filteredItems = computed(() => {
    let items = this.foodItems();
    const cat = this.selectedCategory();
    const q   = this.searchTerm().toLowerCase().trim();
    if (cat !== 'All') items = items.filter(i => i.categoryName === cat);
    if (q) items = items.filter(i => i.itemName.toLowerCase().includes(q));
    return items;
  });

  isUnavailable(item: FoodItem): boolean {
    return !item.isAvailable || item.availableQuantity <= 0;
  }

  cartTotal = computed(() =>
    this.cart().reduce((s, i) => s + (i.price + i.addOnTotal) * i.quantity, 0)
  );

  getCartQty(foodItemId: number): number {
    return this.cart().find(c => c.foodItemId === foodItemId)?.quantity ?? 0;
  }

  tableHasPendingOrder(tableId: number): boolean {
    return this.pendingOrders().some(o => o.tableId === tableId);
  }

  getChairSlots(capacity: number, _side: 'top' | 'bottom'): number[] {
    const count = capacity <= 2 ? 1 : capacity <= 6 ? 2 : 3;
    return Array.from({ length: count }, (_, i) => i);
  }

  ngOnInit(): void {
    forkJoin({
      items:      this.foodItemService.getAll(),
      categories: this.categoryService.getAll(),
      tables:     this.tableService.getAll(),
      pending:    this.orderService.getPending(),
      addOns:     this.addOnService.getAll(),
    }).subscribe({
      next: ({ items, categories, tables, pending, addOns }) => {
        this.foodItems.set(items.data ?? []);
        this.categories.set((categories.data ?? []).filter((c: Category) => c.isActive));
        this.tables.set((tables.data ?? []).filter((t: Table) => t.isActive));
        this.pendingOrders.set(pending.data ?? []);
        this.allAddOns.set((addOns.data ?? []).filter(a => a.isAvailable !== false));
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load data.');
        this.loading.set(false);
      },
    });
    this.startSignalR();
  }

  selectTable(tableId: number): void {
    this.selectedTableId.set(this.selectedTableId() === tableId ? null : tableId);
    this.cart.set([]);
  }

  addToCart(item: FoodItem): void {
    if (!this.selectedTableId()) {
      this.notify.error('Please select a table first.');
      return;
    }
    // Already in cart — just increase quantity, keep existing add-ons
    const existing = this.cart().find(c => c.foodItemId === item.id);
    if (existing) {
      this.cart.update(cart =>
        cart.map(c => c.foodItemId === item.id ? { ...c, quantity: c.quantity + 1 } : c)
      );
      return;
    }
    const matchingAddOns = this.getAddOnsForItem(item);
    if (matchingAddOns.length > 0) {
      this.openAddOnPicker(item, matchingAddOns);
    } else {
      this.addToCartDirect(item, [], '', 0);
    }
  }

  private getAddOnsForItem(item: FoodItem): AddOn[] {
    const all = this.allAddOns();
    if (!all.length) return [];

    const itemCat = item.categoryName.toLowerCase();
    return all.filter(a => {
      if (!a.category) return true; // no category = global, shown for every item
      return a.category.split(',')
        .map(c => c.trim().toLowerCase())
        .some(c => itemCat === c || itemCat.includes(c) || c.includes(itemCat));
    });
  }

  private openAddOnPicker(item: FoodItem, addOns: AddOn[]): void {
    const ref = this.dialog.open(AddOnPickerDialogComponent, {
      data: { item, addOns },
      width: '420px',
    });
    ref.afterClosed().subscribe((result: AddOnPickerResult | null | undefined) => {
      if (result === null || result === undefined) return;
      const ids   = result.selectedAddOns.map(a => a.id);
      const names = result.selectedAddOns.map(a => a.name).join(', ');
      const total = result.selectedAddOns.reduce((s, a) => s + a.price, 0);
      this.addToCartDirect(item, ids, names, total);
    });
  }

  private addToCartDirect(item: FoodItem, addOnIds: number[], addOnNames: string, addOnTotal: number): void {
    this.cart.update(cart => [...cart, {
      foodItemId:       item.id,
      itemName:         item.itemName,
      image:            item.image ?? '',
      price:            item.price,
      quantity:         1,
      selectedAddOnIds: addOnIds,
      addOnNames,
      addOnTotal,
    }]);
  }

  updateQty(foodItemId: number, delta: number): void {
    this.cart.update(cart =>
      cart.map(c => c.foodItemId === foodItemId ? { ...c, quantity: c.quantity + delta } : c)
          .filter(c => c.quantity > 0)
    );
  }

  onSearch(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  sendToKitchen(): void {
    const tableId = this.selectedTableId();
    if (!tableId) { this.notify.error('Select a table first.'); return; }
    if (this.cart().length === 0) { this.notify.error('Add items before sending.'); return; }

    this.sending.set(true);
    const kitchenItems: KitchenItem[] = this.cart().map(c => ({
      name: c.itemName + (c.addOnNames ? ` [${c.addOnNames}]` : ''),
      qty: c.quantity,
    }));
    const items = this.cart().map(c => ({
      foodItemId: c.foodItemId,
      quantity:   c.quantity,
      addOnIds:   c.selectedAddOnIds ?? [],
    }));
    const tableNo = this.selectedTable()?.tableNumber ?? '';
    const existingOrder = this.activeTableOrder();

    const obs = existingOrder
      ? this.orderService.addItems(existingOrder.id, { items })
      : this.orderService.create({ tableId, items, discount: 0 });

    obs.subscribe({
      next: (res: any) => {
        const orderNo = existingOrder?.orderNumber ?? res.data?.orderNumber ?? '';
        this.sending.set(false);
        this.cart.set([]);
        this.refreshPending();
        this.printKitchenSlip(kitchenItems, String(tableNo), orderNo);
        this.notify.success('Sent to kitchen!');
      },
      error: (err: any) => {
        this.sending.set(false);
        this.notify.error(err?.error?.message || 'Failed to send to kitchen.');
      },
    });
  }

  generateBill(): void {
    const order = this.activeTableOrder();
    if (!order) { this.notify.error('No open order for this table.'); return; }
    if (this.cart().length > 0) {
      this.notify.error('Send current items to kitchen before printing bill.');
      return;
    }
    this.generatingBill.set(true);
    this.orderService.generateBill(order.id).subscribe({
      next: (res: any) => {
        this.generatingBill.set(false);
        this.refreshPending();
        this.selectedTableId.set(null);
        this.cart.set([]);
        this.orderService.openReceipt(res.data);
        this.notify.success('Bill generated!');
      },
      error: (err: any) => {
        this.generatingBill.set(false);
        this.notify.error(err?.error?.message || 'Failed to generate bill.');
      },
    });
  }

  private refreshPending(): void {
    this.orderService.getPending().subscribe(res =>
      this.pendingOrders.set(res.data ?? [])
    );
  }

  private startSignalR(): void {
    // Ensure the hub is connected (idempotent — no-op when already connected).
    // Guards against edge cases where LayoutComponent's connection failed silently.
    this.signalR.startConnection();
    this.signalRSub = this.signalR.orderStatusChanged$.subscribe((ev: OrderStatusChangedEvent) => {
      if (ev.status === 'KitchenReady') {
        const tableInfo = ev.tableNumber ? `Table ${ev.tableNumber}` : 'Takeaway';
        this.showNotification(`Order #${ev.orderNumber} (${tableInfo}) is ready for pickup!`, ev.orderId);
        this.refreshPending();
      }
      if (ev.status === 'Pending') {
        // Another device marked it picked up — clear banner if it's the same order
        if (this.notifOrderId() === ev.orderId) this.dismissNotification();
        this.refreshPending();
      }
    });
  }

  showNotification(msg: string, orderId: number): void {
    this.notification.set(msg);
    this.notifOrderId.set(orderId);
    this.notifPickingUp.set(false);
    if (this.notifTimeout) clearTimeout(this.notifTimeout);
    this.notifTimeout = setTimeout(() => this.dismissNotification(), 30000);
  }

  dismissNotification(): void {
    this.notification.set(null);
    this.notifOrderId.set(null);
    this.notifPickingUp.set(false);
    if (this.notifTimeout) clearTimeout(this.notifTimeout);
  }

  markPickedUp(): void {
    const orderId = this.notifOrderId();
    if (!orderId || this.notifPickingUp()) return;
    this.notifPickingUp.set(true);
    this.orderService.markPickedUp(orderId).subscribe({
      next: () => this.dismissNotification(),
      error: (err: any) => {
        this.notifPickingUp.set(false);
        this.notify.error(err?.error?.message ?? 'Failed to mark as picked up.');
      },
    });
  }

  openSplitModal(): void {
    const order = this.activeTableOrder();
    if (!order) { this.notify.error('No active order for this table.'); return; }
    this.splitPendingOrder.set(order);
    this.splitCount.set(2);
    this.showSplitModal.set(true);
  }

  closeSplitModal(): void {
    this.showSplitModal.set(false);
    this.splitPendingOrder.set(null);
  }

  splitAmount(): number {
    const order = this.splitPendingOrder();
    if (!order) return 0;
    const count = Math.max(this.splitCount(), 1);
    return Math.ceil(order.grandTotal / count);
  }

  ngOnDestroy(): void {
    this.signalRSub?.unsubscribe();
    if (this.notifTimeout) clearTimeout(this.notifTimeout);
  }

  private printKitchenSlip(items: KitchenItem[], tableNo: string, orderNo: string): void {
    const rows = items.map(i =>
      `<tr>
        <td style="padding:7px 0;font-size:1rem;border-bottom:1px solid #eee">${i.name}</td>
        <td style="text-align:center;font-size:1.5rem;font-weight:800;padding:7px 0;border-bottom:1px solid #eee">${i.qty}</td>
      </tr>`
    ).join('');

    const html = `<!DOCTYPE html><html><head><title>Kitchen Slip</title>
<style>
  *{margin:0;padding:0;box-sizing:border-box}
  body{font-family:'Courier New',monospace;width:280px;padding:18px 14px;background:#fff}
  .title{text-align:center;font-size:1.15rem;font-weight:700;letter-spacing:2px;margin-bottom:4px}
  .meta{text-align:center;font-size:.85rem;color:#444;margin-bottom:4px}
  .time{text-align:center;font-size:.78rem;color:#888;margin-bottom:10px}
  hr{border:none;border-top:2px dashed #999;margin:10px 0}
  table{width:100%;border-collapse:collapse}
  thead th{font-size:.72rem;text-transform:uppercase;color:#999;padding-bottom:8px}
  .footer{text-align:center;font-size:.72rem;color:#aaa;margin-top:14px;letter-spacing:1px}
</style></head><body>
<div class="title">★ KITCHEN ORDER ★</div>
<div class="meta">Table <strong>${tableNo}</strong> &nbsp;|&nbsp; #${orderNo}</div>
<div class="time">${new Date().toLocaleTimeString()}</div>
<hr/>
<table>
  <thead><tr><th style="text-align:left">Item</th><th>Qty</th></tr></thead>
  <tbody>${rows}</tbody>
</table>
<hr/>
<div class="footer">PREPARE IMMEDIATELY</div>
</body></html>`;

    const win = window.open('', '_blank', 'width=340,height=540');
    if (win) {
      win.document.write(html);
      win.document.close();
      setTimeout(() => { win.print(); win.close(); }, 400);
    }
  }
}
