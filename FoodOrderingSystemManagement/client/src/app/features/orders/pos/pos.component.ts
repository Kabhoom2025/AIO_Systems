import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../../core/services/signalr.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { FoodItemService } from '../../../core/services/food-item.service';
import { CategoryService } from '../../../core/services/category.service';
import { CartService } from '../../../core/services/cart.service';
import { TableService } from '../../../core/services/table.service';
import { AddOnService } from '../../../core/services/addon.service';
import { NotificationService } from '../../../shared/services/notification.service';
import { FoodItem } from '../../../core/models/food-item.model';
import { Category } from '../../../core/models/category.model';
import { CartItem, Order } from '../../../core/models/order.model';
import { Table, HallType } from '../../../core/models/table.model';
import { AddOn } from '../../../core/models/addon.model';
import { OrderService } from '../../../core/services/order.service';
import { WhatsAppService } from '../../../core/services/whatsapp.service';
import { TakeawayPhoneDialogComponent, TakeawayPhoneResult } from '../takeaway-phone-dialog/takeaway-phone-dialog.component';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { BillDialogComponent, BillDialogResult } from './bill-dialog.component';
import { PaymentQrDialogComponent, PaymentQrDialogData } from '../payment-qr-dialog/payment-qr-dialog.component';
import { AddOnPickerDialogComponent, AddOnPickerResult } from './addon-picker-dialog.component';
import { BarcodeService } from '../../../core/services/barcode.service';
import { BarcodeScannerComponent } from '../../../shared/components/barcode-scanner/barcode-scanner.component';
import { CustomerService } from '../../../core/services/customer.service';
import { Customer } from '../../../core/models/customer.model';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    BarcodeScannerComponent,
  ],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.scss',
})
export class PosComponent implements OnInit, OnDestroy {
  private foodItemService = inject(FoodItemService);
  private categoryService = inject(CategoryService);
  private orderService    = inject(OrderService);
  private tableService    = inject(TableService);
  private addOnService    = inject(AddOnService);
  private whatsapp        = inject(WhatsAppService);
  private dialog          = inject(MatDialog);
  private notify          = inject(NotificationService);
  private barcodeService  = inject(BarcodeService);
  private customerService = inject(CustomerService);
  private signalR         = inject(SignalRService);
  readonly cart           = inject(CartService);

  private barcodeSub:   Subscription | null = null;
  private signalRSubs:  Subscription[]      = [];

  /** Stores whole-order rejection info keyed by tableId so the panel survives a pendingOrders reload. */
  rejectedByTable = signal<Map<number, { orderId: number; orderNumber: string; reason: string; itemNames: string[] }>>(new Map());

  /** Stores partial item rejection info keyed by orderId. Cleared when cashier dismisses. */
  partialRejectedByOrder = signal<Map<number, { rejectedItems: string[]; reason: string }>>(new Map());

  allItems      = signal<FoodItem[]>([]);
  categories    = signal<Category[]>([]);
  tables        = signal<Table[]>([]);
  pendingOrders = signal<Order[]>([]);
  allAddOns     = signal<AddOn[]>([]);
  loading       = signal(false);
  placing       = signal(false);
  searchTerm    = signal('');
  selectedCategoryId = signal<number | null>(null);
  selectedTableId    = signal<number | null>(null);
  selectedHall       = signal<HallType>('AC');
  visibleTables      = computed(() => this.tables().filter(t => t.hall === this.selectedHall()));
  discountInput = signal('0');
  discountMode  = signal<'amount' | 'percent'>('amount');

  // Order mode
  orderMode = signal<'dinein' | 'takeaway' | 'delivery'>('dinein');

  // Delivery fields
  deliveryAddress = signal('');
  deliveryPhone   = signal('');
  deliveryCharge  = signal(0);

  // Customer / loyalty
  customerPhoneInput  = signal('');
  customer            = signal<Customer | null>(null);
  lookingUpCustomer   = signal(false);
  pointsToRedeem      = signal(0);

  filteredItems = computed(() => {
    let items = this.allItems().filter((i) => i.isAvailable);
    const cat  = this.selectedCategoryId();
    const term = this.searchTerm().toLowerCase();
    if (cat !== null) items = items.filter((i) => i.categoryId === cat);
    if (term)         items = items.filter((i) => i.itemName.toLowerCase().includes(term));
    return items;
  });

  /** The open (Pending) order for the currently selected table, if any. */
  activeTableOrder = computed<Order | null>(() => {
    const tableId = this.selectedTableId();
    if (tableId === null) return null;
    return this.pendingOrders().find((o) => o.tableId === tableId) ?? null;
  });

  /** True when adding to an existing dine-in order. */
  isAddingToOrder = computed(() => this.activeTableOrder() !== null);

  tableHasPendingOrder(tableId: number): boolean {
    return this.pendingOrders().some((o) => o.tableId === tableId);
  }

  isAtMaxStock(item: { id: number; availableQuantity: number }): boolean {
    return this.cart.quantityOf(item.id) >= item.availableQuantity;
  }

  isOutOfStock(item: { availableQuantity: number }): boolean {
    return item.availableQuantity === 0;
  }

  ngOnInit(): void {
    this.load();
    this.barcodeSub = this.barcodeService.scan$.subscribe(code => this.onBarcodeScanned(code));
    this.signalRSubs.push(
      this.signalR.orderRejectedByKitchen$.subscribe(ev => {
        // Capture item names BEFORE reloading (the order disappears from DB)
        const order = this.pendingOrders().find(o => o.id === ev.orderId);
        const tableId = order?.tableId ?? null;
        const itemNames = (order?.items ?? []).map((i: any) => `${i.itemName} ×${i.quantity}`);
        if (tableId !== null) {
          this.rejectedByTable.update(m => new Map(m).set(tableId, {
            orderId:     ev.orderId,
            orderNumber: ev.orderNumber,
            reason:      ev.reason,
            itemNames,
          }));
        }
        // Reload so cancelled order disappears from pendingOrders & table dot clears
        this.orderService.getPending().subscribe(r => this.pendingOrders.set(r.data ?? []));
      }),
      this.signalR.orderItemsRejected$.subscribe(ev => {
        if (ev.orderCancelled) {
          // All items were rejected → treat like full order rejection
          const order = this.pendingOrders().find(o => o.id === ev.orderId);
          const tableId = order?.tableId ?? (ev.tableNumber ? null : null);
          if (tableId !== null && tableId !== undefined) {
            this.rejectedByTable.update(m => new Map(m).set(tableId, {
              orderId:     ev.orderId,
              orderNumber: ev.orderNumber,
              reason:      ev.reason,
              itemNames:   ev.rejectedItems,
            }));
          }
        } else {
          // Partial rejection — keep order open, just mark which items were removed
          this.partialRejectedByOrder.update(m => new Map(m).set(ev.orderId, {
            rejectedItems: ev.rejectedItems,
            reason:        ev.reason,
          }));
        }
        // Reload pending orders so the order reflects updated item list / totals
        this.orderService.getPending().subscribe(r => this.pendingOrders.set(r.data ?? []));
      }),
    );
  }

  ngOnDestroy(): void {
    this.barcodeSub?.unsubscribe();
    this.signalRSubs.forEach(s => s.unsubscribe());
  }

  /** Cashier acknowledges the whole-order rejection — clears the panel for that table. */
  dismissRejection(tableId: number): void {
    this.rejectedByTable.update(m => { const n = new Map(m); n.delete(tableId); return n; });
  }

  rejectionForTable(tableId: number | null) {
    if (tableId === null) return null;
    return this.rejectedByTable().get(tableId) ?? null;
  }

  /** Cashier acknowledges the partial item rejection — clears the banner for that order. */
  dismissPartialRejection(orderId: number): void {
    this.partialRejectedByOrder.update(m => { const n = new Map(m); n.delete(orderId); return n; });
  }

  partialRejectionForOrder(orderId: number | null) {
    if (orderId === null) return null;
    return this.partialRejectedByOrder().get(orderId) ?? null;
  }

  onBarcodeScanned(code: string): void {
    const existing = this.allItems().find(i => i.barcode === code);
    if (existing) {
      this.addToCart(existing);
      this.notify.success(`Added: ${existing.itemName}`);
      return;
    }
    // Not in local cache — fetch from API (handles items loaded after init)
    this.foodItemService.getByBarcode(code).subscribe({
      next: res => {
        if (res.data) {
          this.addToCart(res.data);
          this.notify.success(`Added: ${res.data.itemName}`);
        }
      },
      error: () => this.notify.error(`No item found for barcode: ${code}`),
    });
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      items:         this.foodItemService.getAll(),
      categories:    this.categoryService.getAll(),
      tables:        this.tableService.getAll(),
      pendingOrders: this.orderService.getPending(),
      addOns:        this.addOnService.getAll(),
    }).subscribe({
      next: ({ items, categories, tables, pendingOrders, addOns }) => {
        this.allItems.set(items.data ?? []);
        this.categories.set((categories.data ?? []).filter((c) => c.isActive));
        this.tables.set((tables.data ?? []).filter((t) => t.isActive));
        this.pendingOrders.set(pendingOrders.data ?? []);
        this.allAddOns.set((addOns.data ?? []).filter(a => a.isAvailable !== false));
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load menu.');
        this.loading.set(false);
      },
    });
  }

  selectCategory(id: number | null): void {
    this.selectedCategoryId.set(id);
  }

  addToCart(item: FoodItem): void {
    if (item.availableQuantity === 0) {
      this.notify.warn(`"${item.itemName}" is out of stock.`);
      return;
    }
    if (this.cart.quantityOf(item.id) >= item.availableQuantity) {
      this.notify.warn(`Only ${item.availableQuantity} unit(s) available for "${item.itemName}".`);
      return;
    }
    // Already in cart — just increase quantity without re-opening add-on dialog
    if (this.cart.isInCart(item.id)) {
      this.cart.increase(item.id);
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
    const cartItem: CartItem = {
      foodItemId:       item.id,
      itemName:         item.itemName,
      image:            item.image,
      price:            item.price,
      quantity:         1,
      selectedAddOnIds: addOnIds,
      addOnNames,
      addOnTotal,
    };
    this.cart.add(cartItem);
  }

  increaseInCart(foodItemId: number, itemName: string): void {
    const stock = this.allItems().find((i) => i.id === foodItemId)?.availableQuantity ?? 0;
    if (this.cart.quantityOf(foodItemId) >= stock) {
      this.notify.warn(`Only ${stock} unit(s) available for "${itemName}".`);
      return;
    }
    this.cart.increase(foodItemId);
  }

  toggleDiscountMode(): void {
    this.discountMode.set(this.discountMode() === 'amount' ? 'percent' : 'amount');
    this.discountInput.set('0');
    this.cart.setDiscount(0);
  }

  applyDiscount(): void {
    const val = parseFloat(this.discountInput());
    if (isNaN(val) || val < 0) { this.cart.setDiscount(0); return; }
    if (this.discountMode() === 'percent') {
      const pct = Math.min(val, 100);
      this.cart.setDiscount(parseFloat(((this.cart.subTotal() * pct) / 100).toFixed(2)));
    } else {
      this.cart.setDiscount(val);
    }
  }

  lookupCustomer(): void {
    const phone = this.customerPhoneInput().trim();
    if (!phone) return;
    this.lookingUpCustomer.set(true);
    this.customerService.getByPhone(phone).subscribe({
      next: res => {
        this.customer.set(res.data ?? null);
        this.lookingUpCustomer.set(false);
        if (!res.data) this.notify.warn('No customer found for that phone number.');
      },
      error: () => {
        this.customer.set(null);
        this.lookingUpCustomer.set(false);
        this.notify.warn('No customer found for that phone number.');
      },
    });
  }

  clearCustomer(): void {
    this.customer.set(null);
    this.customerPhoneInput.set('');
    this.pointsToRedeem.set(0);
  }

  tierIcon(tier: string): string {
    return tier === 'Gold' ? 'emoji_events' : tier === 'Silver' ? 'workspace_premium' : 'military_tech';
  }

  clearCart(): void {
    if (this.cart.isEmpty()) return;
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Clear Items',
        message: 'Remove all items from the current order?',
        confirmText: 'Clear',
        danger: true,
      },
      width: '400px',
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.cart.clear();
        this.discountInput.set('0');
      }
    });
  }

  setOrderMode(mode: 'dinein' | 'takeaway' | 'delivery'): void {
    this.orderMode.set(mode);
    if (mode !== 'dinein') this.selectedTableId.set(null);
  }

  /** Place a new order (dine-in, takeaway or delivery). */
  placeOrder(): void {
    if (this.cart.isEmpty()) return;
    const mode    = this.orderMode();
    const tableId = this.selectedTableId();

    if (mode === 'delivery') {
      if (!this.deliveryAddress().trim()) {
        this.notify.warn('Please enter a delivery address.');
        return;
      }
      this.doPlaceOrder(null, this.deliveryPhone() || null, 'Delivery');
    } else if (mode === 'takeaway' || (!tableId && mode === 'dinein')) {
      // Takeaway: show phone dialog first
      const ref = this.dialog.open(TakeawayPhoneDialogComponent, { width: '400px', disableClose: false });
      ref.afterClosed().subscribe((result: TakeawayPhoneResult | null) => {
        if (result === undefined || result === null) return;
        this.doPlaceOrder(null, result.phone, 'Takeaway');
      });
    } else {
      // Dine-in: place directly
      this.doPlaceOrder(tableId, null, 'DineIn');
    }
  }

  private doPlaceOrder(tableId: number | null, customerPhone: string | null, orderType: 'DineIn' | 'Takeaway' | 'Delivery' = 'Takeaway'): void {
    this.placing.set(true);
    const cust = this.customer();
    const payload: any = {
      tableId,
      customerPhone,
      customerId:     cust?.id ?? undefined,
      pointsRedeemed: cust ? this.pointsToRedeem() : 0,
      discount: this.cart.discount(),
      orderType,
      items: this.cart.items().map((i) => ({
        foodItemId: i.foodItemId,
        quantity:   i.quantity,
        addOnIds:   i.selectedAddOnIds ?? [],
      })),
    };
    if (orderType === 'Delivery') {
      payload.deliveryAddress = this.deliveryAddress().trim();
      payload.deliveryCharge  = this.deliveryCharge();
    }

    this.orderService.create(payload).subscribe({
      next: (res) => {
        this.placing.set(false);
        if (res.success) {
          this.cart.clear();
          this.discountInput.set('0');

          if (orderType === 'Delivery') {
            // Delivery: pending in delivery dashboard
            this.notify.success(`Delivery order ${res.data.orderNumber} created. Assign a driver in the Delivery Dashboard.`);
            this.deliveryAddress.set('');
            this.deliveryPhone.set('');
            this.deliveryCharge.set(0);
            this.clearCustomer();
          } else if (tableId || orderType === 'DineIn') {
            // Dine-in: order stays Pending
            this.notify.success(`Order ${res.data.orderNumber} opened. Add more items or generate the bill when ready.`);
            this.orderService.getPending().subscribe((r) => this.pendingOrders.set(r.data ?? []));
          } else {
            // Takeaway: completed — send WhatsApp then show receipt
            this.selectedTableId.set(null);
            this.clearCustomer();
            if (customerPhone) {
              this.whatsapp.sendOrderConfirmation(customerPhone, {
                orderNumber: res.data.orderNumber,
                items: res.data.items ?? [],
                subTotal: res.data.subTotal,
                tax: res.data.tax,
                discount: res.data.discount,
                grandTotal: res.data.grandTotal,
                restaurantName: res.data.restaurantName,
              });
            }
            this.orderService.openReceipt(res.data);
          }
        } else {
          this.notify.error(res.message || 'Order failed.');
        }
      },
      error: (err) => {
        this.placing.set(false);
        this.notify.error(err?.error?.message || 'Failed to place order.');
      },
    });
  }

  /** Add cart items to the existing pending dine-in order for this table. */
  addToExistingOrder(): void {
    const active = this.activeTableOrder();
    if (!active || this.cart.isEmpty()) return;
    this.placing.set(true);

    const payload = {
      items: this.cart.items().map((i) => ({
        foodItemId: i.foodItemId,
        quantity:   i.quantity,
        addOnIds:   i.selectedAddOnIds ?? [],
      })),
      discount: this.cart.discount(),
    };

    this.orderService.addItems(active.id, payload).subscribe({
      next: (res) => {
        this.placing.set(false);
        if (res.success) {
          this.notify.success('Items added to the order.');
          this.cart.clear();
          this.discountInput.set('0');
          this.orderService.getPending().subscribe((r) => this.pendingOrders.set(r.data ?? []));
        } else {
          this.notify.error(res.message || 'Failed to add items.');
        }
      },
      error: (err) => {
        this.placing.set(false);
        this.notify.error(err?.error?.message || 'Failed to add items.');
      },
    });
  }

  /** Finalize the open dine-in order — cashier/admin can set a final discount. */
  generateBill(): void {
    const active = this.activeTableOrder();
    if (!active) return;

    const ref = this.dialog.open(BillDialogComponent, {
      data: {
        orderNumber:     active.orderNumber,
        tableNumber:     active.tableNumber,
        subTotal:        active.subTotal,
        tax:             active.tax,
        currentDiscount: active.discount,
      },
      width: '420px',
      disableClose: false,
    });

    ref.afterClosed().subscribe((result: BillDialogResult | null) => {
      if (result === null || result === undefined) return;
      this.placing.set(true);

      this.orderService.generateBill(active.id, result.discount).subscribe({
        next: (res) => {
          this.placing.set(false);
          if (res.success) {
            this.notify.success(`Bill generated for ${active.orderNumber}.`);
            this.cart.clear();
            this.discountInput.set('0');
            this.selectedTableId.set(null);
            this.orderService.openReceipt(res.data);
            const payRef = this.dialog.open(PaymentQrDialogComponent, {
              data: {
                orderId:        res.data.orderId,
                orderNumber:    res.data.orderNumber,
                grandTotal:     res.data.grandTotal,
                upiId:          res.data.upiId,
                restaurantName: res.data.restaurantName,
              } as PaymentQrDialogData,
              width: '400px',
            });
            payRef.afterClosed().subscribe(() => {
              this.orderService.getPending().subscribe((r) => this.pendingOrders.set(r.data ?? []));
            });
          } else {
            this.notify.error(res.message || 'Failed to generate bill.');
          }
        },
        error: (err) => {
          this.placing.set(false);
          this.notify.error(err?.error?.message || 'Failed to generate bill.');
        },
      });
    });
  }

  getCategoryIcon(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('pizza'))    return '🍕';
    if (n.includes('burger'))   return '🍔';
    if (n.includes('biryani'))  return '🍛';
    if (n.includes('drink'))    return '🥤';
    if (n.includes('ice'))      return '🍦';
    if (n.includes('dessert'))  return '🍰';
    if (n.includes('chinese'))  return '🍜';
    if (n.includes('sandwich')) return '🥪';
    if (n.includes('chicken'))  return '🍗';
    if (n.includes('coffee'))   return '☕';
    return '🍽️';
  }
}
