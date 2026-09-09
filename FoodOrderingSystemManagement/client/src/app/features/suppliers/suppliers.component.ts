import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRippleModule } from '@angular/material/core';
import { SupplierService } from '../../core/services/supplier.service';
import { InventoryService } from '../../core/services/inventory.service';
import { Supplier, PurchaseOrder, SupplierPayment, PO_STATUSES } from '../../core/models/supplier.model';
import { InventoryItem } from '../../core/models/inventory.model';
import { SupplierFormDialogComponent } from './supplier-form-dialog.component';
import { PurchaseOrderDialogComponent } from './purchase-order-dialog.component';
import { SupplierPaymentDialogComponent } from './supplier-payment-dialog.component';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, MatTooltipModule,
            MatProgressSpinnerModule, MatFormFieldModule, MatInputModule, MatRippleModule],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss',
})
export class SuppliersComponent implements OnInit {
  private supplierSvc  = inject(SupplierService);
  private inventorySvc = inject(InventoryService);
  private dialog       = inject(MatDialog);
  private snack        = inject(MatSnackBar);

  suppliers      = signal<Supplier[]>([]);
  inventoryItems = signal<InventoryItem[]>([]);
  loading        = signal(false);
  searchTerm     = signal('');
  filterActive   = signal<'all' | 'active' | 'inactive'>('all');
  selectedId     = signal<number | null>(null);
  purchaseOrders = signal<PurchaseOrder[]>([]);
  payments       = signal<SupplierPayment[]>([]);
  detailTab      = signal<'orders' | 'payments'>('orders');
  detailLoading  = signal(false);

  filtered = computed(() => {
    let list = this.suppliers();
    const q = this.searchTerm().toLowerCase();
    if (q) list = list.filter(s => s.name.toLowerCase().includes(q) || s.contactPerson?.toLowerCase().includes(q) || s.email?.toLowerCase().includes(q));
    if (this.filterActive() === 'active')   list = list.filter(s => s.isActive);
    if (this.filterActive() === 'inactive') list = list.filter(s => !s.isActive);
    return list;
  });

  selectedSupplier = computed(() => this.suppliers().find(s => s.id === this.selectedId()) ?? null);
  totalSuppliers   = computed(() => this.suppliers().length);
  activeCount      = computed(() => this.suppliers().filter(s => s.isActive).length);
  totalPaid        = computed(() => this.suppliers().reduce((s, x) => s + x.totalPaid, 0));

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.supplierSvc.getAll().subscribe({
      next: s => { this.suppliers.set(s); this.loading.set(false); },
      error: () => { this.loading.set(false); this.snack.open('Failed to load suppliers', '', { duration: 3000, panelClass: 'snack-error' }); },
    });
    this.inventorySvc.getAll().subscribe({ next: res => this.inventoryItems.set(res.data ?? []) });
  }

  selectSupplier(id: number): void {
    if (this.selectedId() === id) { this.selectedId.set(null); return; }
    this.selectedId.set(id);
    this.loadDetail(id);
  }

  loadDetail(id: number): void {
    this.detailLoading.set(true);
    this.purchaseOrders.set([]);
    this.payments.set([]);
    this.supplierSvc.getPurchaseOrders(id).subscribe({ next: o => this.purchaseOrders.set(o) });
    this.supplierSvc.getPayments(id).subscribe({
      next: p => { this.payments.set(p); this.detailLoading.set(false); },
      error: () => this.detailLoading.set(false),
    });
  }

  openCreate(): void {
    this.dialog.open(SupplierFormDialogComponent, { data: {}, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.supplierSvc.create(result).subscribe({
          next: s => { this.suppliers.update(list => [...list, s]); this.snack.open('Supplier created', '', { duration: 2500, panelClass: 'snack-success' }); },
          error: () => this.snack.open('Failed to create supplier', '', { duration: 3000, panelClass: 'snack-error' }),
        });
      });
  }

  openEdit(supplier: Supplier): void {
    this.dialog.open(SupplierFormDialogComponent, { data: { supplier }, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.supplierSvc.update(supplier.id, result).subscribe({
          next: updated => { this.suppliers.update(list => list.map(s => s.id === updated.id ? updated : s)); this.snack.open('Supplier updated', '', { duration: 2500, panelClass: 'snack-success' }); },
          error: () => this.snack.open('Failed to update supplier', '', { duration: 3000, panelClass: 'snack-error' }),
        });
      });
  }

  confirmDelete(supplier: Supplier): void {
    if (!confirm(`Delete "${supplier.name}"? This cannot be undone.`)) return;
    this.supplierSvc.delete(supplier.id).subscribe({
      next: () => { this.suppliers.update(list => list.filter(s => s.id !== supplier.id)); if (this.selectedId() === supplier.id) this.selectedId.set(null); this.snack.open('Supplier deleted', '', { duration: 2500, panelClass: 'snack-success' }); },
      error: () => this.snack.open('Failed to delete supplier', '', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  openPurchaseOrder(supplier: Supplier): void {
    this.dialog.open(PurchaseOrderDialogComponent, { data: { supplier, inventoryItems: this.inventoryItems() }, disableClose: true, width: '720px' })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.supplierSvc.createPurchaseOrder(result).subscribe({
          next: po => {
            this.purchaseOrders.update(list => [po, ...list]);
            this.suppliers.update(list => list.map(s => s.id === supplier.id ? { ...s, purchaseOrderCount: s.purchaseOrderCount + 1 } : s));
            this.snack.open(`PO ${po.poNumber} created`, '', { duration: 3000, panelClass: 'snack-success' });
          },
          error: () => this.snack.open('Failed to create purchase order', '', { duration: 3000, panelClass: 'snack-error' }),
        });
      });
  }

  openPayment(supplier: Supplier): void {
    const pos = this.purchaseOrders().filter(po => po.status !== 'Cancelled');
    this.dialog.open(SupplierPaymentDialogComponent, { data: { supplier, purchaseOrders: pos }, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.supplierSvc.createPayment(result).subscribe({
          next: payment => {
            this.payments.update(list => [payment, ...list]);
            this.suppliers.update(list => list.map(s => s.id === supplier.id ? { ...s, totalPaid: s.totalPaid + payment.amount } : s));
            this.snack.open('Payment recorded', '', { duration: 2500, panelClass: 'snack-success' });
          },
          error: () => this.snack.open('Failed to record payment', '', { duration: 3000, panelClass: 'snack-error' }),
        });
      });
  }

  updatePoStatus(po: PurchaseOrder, status: string): void {
    this.supplierSvc.updatePurchaseOrderStatus(po.id, { status }).subscribe({
      next: updated => this.purchaseOrders.update(list => list.map(p => p.id === updated.id ? updated : p)),
      error: () => this.snack.open('Failed to update status', '', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  poStatusColor(status: string): string {
    const map: Record<string, string> = { Draft: '#9e9e9e', Sent: '#1565c0', Confirmed: '#6a1b9a', PartiallyReceived: '#e65100', Received: '#2e7d32', Cancelled: '#c62828' };
    return map[status] ?? '#9e9e9e';
  }

  poStatusBg(status: string): string {
    const map: Record<string, string> = { Draft: '#f5f5f5', Sent: '#e3f2fd', Confirmed: '#f3e5f5', PartiallyReceived: '#fff3e0', Received: '#e8f5e9', Cancelled: '#ffebee' };
    return map[status] ?? '#f5f5f5';
  }

  nextStatuses(current: string): string[] {
    const flows: Record<string, string[]> = {
      Draft: ['Sent', 'Cancelled'],
      Sent: ['Confirmed', 'Cancelled'],
      Confirmed: ['PartiallyReceived', 'Received', 'Cancelled'],
      PartiallyReceived: ['Received', 'Cancelled'],
      Received: [],
      Cancelled: [],
    };
    return flows[current] ?? [];
  }
}
