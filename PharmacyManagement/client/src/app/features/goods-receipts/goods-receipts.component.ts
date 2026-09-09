import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';

interface PurchaseOrderItem {
  id: number; medicineId: number; medicineName: string;
  quantity: number; unitPrice: number; receivedQuantity: number;
}
interface PurchaseOrder {
  id: number; poNumber: string; supplierName: string; status: string; items: PurchaseOrderItem[];
}
interface GoodsReceiptItem {
  id: number; medicineName: string; batchNumber: string; expiryDate: string;
  quantityReceived: number; purchasePrice: number;
}
interface GoodsReceipt {
  id: number; receiptNumber: string; poNumber: string; receivedDate: string;
  receivedBy?: string; items: GoodsReceiptItem[];
}

interface ReceiptLine {
  purchaseOrderItemId: number; medicineId: number; medicineName: string;
  batchNumber: string; expiryDate: string; manufacturingDate: string;
  quantityReceived: number; purchasePrice: number;
}

@Component({
  selector: 'app-goods-receipts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './goods-receipts.component.html',
  styleUrl: './goods-receipts.component.scss'
})
export class GoodsReceiptsComponent implements OnInit {
  goodsReceipts = signal<GoodsReceipt[]>([]);
  openOrders    = signal<PurchaseOrder[]>([]);
  loading       = signal(false);
  showForm      = signal(false);
  selectedPoId  = signal<number | null>(null);

  orgId = getOrgIdFromToken();

  receivedBy = '';
  lines: ReceiptLine[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadReceipts();
    this.loadOpenOrders();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadReceipts() {
    this.loading.set(true);
    this.http.get<GoodsReceipt[]>(`${environment.apiUrl}/goods-receipts/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.goodsReceipts.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadOpenOrders() {
    this.http.get<PurchaseOrder[]>(`${environment.apiUrl}/purchase-orders/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.openOrders.set(data.filter(p => p.status === 'Sent' || p.status === 'PartiallyReceived')));
  }

  openAdd() {
    this.selectedPoId.set(null);
    this.receivedBy = '';
    this.lines = [];
    this.showForm.set(true);
  }

  onSelectPo(poId: number | null) {
    this.selectedPoId.set(poId);
    const po = this.openOrders().find(p => p.id === poId);
    this.lines = po
      ? po.items
          .filter(i => i.receivedQuantity < i.quantity)
          .map(i => ({
            purchaseOrderItemId: i.id,
            medicineId: i.medicineId,
            medicineName: i.medicineName,
            batchNumber: '',
            expiryDate: '',
            manufacturingDate: '',
            quantityReceived: i.quantity - i.receivedQuantity,
            purchasePrice: i.unitPrice
          }))
      : [];
  }

  save() {
    const poId = this.selectedPoId();
    if (!poId || this.lines.length === 0) return;
    const dto = {
      purchaseOrderId: poId,
      receivedBy: this.receivedBy,
      items: this.lines.map(l => ({
        purchaseOrderItemId: l.purchaseOrderItemId,
        medicineId: l.medicineId,
        batchNumber: l.batchNumber,
        expiryDate: l.expiryDate,
        manufacturingDate: l.manufacturingDate || null,
        quantityReceived: l.quantityReceived,
        purchasePrice: l.purchasePrice
      }))
    };
    this.http.post(`${environment.apiUrl}/goods-receipts/org/${this.orgId}`, dto, {
      headers: this.authHeaders()
    }).subscribe(() => {
      this.showForm.set(false);
      this.loadReceipts();
      this.loadOpenOrders();
    });
  }
}
