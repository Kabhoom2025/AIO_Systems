import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getBranchIdFromToken } from '../../core/auth.helper';
import { NotificationService } from '../../core/notification.service';

interface Medicine {
  id: number; name: string; mrp: number; gstPercent: number; totalStock: number; unit: string;
}
interface Customer { id: number; name: string; phone?: string; }

interface CartLine {
  medicineId: number; name: string; unit: string; quantity: number;
  unitPrice: number; discountAmount: number; gstPercent: number;
}

interface Receipt {
  invoiceNumber: string; saleDate: string; totalAmount: number;
  subtotal: number; taxAmount: number; discountAmount: number; paymentMethod: string;
}

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.scss'
})
export class PosComponent implements OnInit {
  medicines = signal<Medicine[]>([]);
  customers = signal<Customer[]>([]);
  cart = signal<CartLine[]>([]);
  receipt = signal<Receipt | null>(null);
  submitting = signal(false);

  orgId = getOrgIdFromToken();
  branchId = getBranchIdFromToken();

  searchTerm = '';
  scanCode = '';
  selectedMedicineId: number | null = null;
  addQuantity = 1;
  selectedCustomerId: number | null = null;
  paymentMethod = 'Cash';
  paymentOptions = ['Cash', 'Card', 'UPI', 'Wallet', 'Credit'];

  constructor(private http: HttpClient, private notify: NotificationService) {}

  ngOnInit() {
    this.loadMedicines();
    this.loadCustomers();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadMedicines() {
    this.http.get<Medicine[]>(`${environment.apiUrl}/medicines/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.medicines.set(data));
  }

  loadCustomers() {
    this.http.get<Customer[]>(`${environment.apiUrl}/customers/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.customers.set(data));
  }

  get filteredMedicines(): Medicine[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.medicines();
    return this.medicines().filter(m => m.name.toLowerCase().includes(term));
  }

  addToCart(medicine: Medicine, quantity = this.addQuantity) {
    if (quantity <= 0) return;
    const existing = this.cart().find(l => l.medicineId === medicine.id);
    if (existing) {
      existing.quantity += quantity;
      this.cart.set([...this.cart()]);
    } else {
      this.cart.set([...this.cart(), {
        medicineId: medicine.id, name: medicine.name, unit: medicine.unit,
        quantity, unitPrice: medicine.mrp,
        discountAmount: 0, gstPercent: medicine.gstPercent
      }]);
    }
    this.selectedMedicineId = null;
    this.addQuantity = 1;
    this.searchTerm = '';
  }

  scanBarcode() {
    const code = this.scanCode.trim();
    this.scanCode = '';
    if (!code) return;

    this.http.get<Medicine>(`${environment.apiUrl}/medicines/barcode/${encodeURIComponent(code)}/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: medicine => {
        this.addToCart(medicine, 1);
        this.notify.success(`Added ${medicine.name}.`);
      },
      error: () => this.notify.error(`No medicine found for code "${code}".`)
    });
  }

  removeLine(medicineId: number) {
    this.cart.set(this.cart().filter(l => l.medicineId !== medicineId));
  }

  lineTotal(line: CartLine): number {
    const taxable = line.quantity * line.unitPrice - line.discountAmount;
    return taxable + (taxable * line.gstPercent / 100);
  }

  get grandTotal(): number {
    return this.cart().reduce((sum, l) => sum + this.lineTotal(l), 0);
  }

  checkout() {
    if (this.cart().length === 0) return;
    if (!this.branchId) {
      this.notify.error('No branch assigned to your account — cannot complete a sale.');
      return;
    }

    const dto = {
      branchId: this.branchId,
      customerId: this.selectedCustomerId,
      paymentMethod: this.paymentMethod,
      items: this.cart().map(l => ({
        medicineId: l.medicineId, quantity: l.quantity, discountAmount: l.discountAmount
      }))
    };

    this.submitting.set(true);
    this.http.post<any>(`${environment.apiUrl}/sales/org/${this.orgId}`, dto, {
      headers: this.authHeaders()
    }).subscribe({
      next: sale => {
        this.submitting.set(false);
        this.receipt.set(sale);
        this.cart.set([]);
        this.selectedCustomerId = null;
        this.paymentMethod = 'Cash';
        this.notify.success(`Sale ${sale.invoiceNumber} completed.`);
        this.loadMedicines();
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.notify.error(err.error?.message || err.error || 'Checkout failed.');
      }
    });
  }

  closeReceipt() {
    this.receipt.set(null);
  }

  printReceipt() {
    window.print();
  }
}
