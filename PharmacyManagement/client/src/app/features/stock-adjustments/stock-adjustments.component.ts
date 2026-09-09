import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';

interface MedicineBatch { id: number; batchNumber: string; currentQuantity: number; }
interface Medicine { id: number; name: string; batches: MedicineBatch[]; }

interface StockAdjustment {
  id: number; medicineName: string; batchNumber: string; adjustmentType: string;
  quantity: number; reason: string; notes?: string; adjustedDate: string;
}

@Component({
  selector: 'app-stock-adjustments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-adjustments.component.html',
  styleUrl: './stock-adjustments.component.scss'
})
export class StockAdjustmentsComponent implements OnInit {
  adjustments = signal<StockAdjustment[]>([]);
  medicines   = signal<Medicine[]>([]);
  loading     = signal(false);
  showForm    = signal(false);

  orgId = getOrgIdFromToken();
  reasonOptions = ['Damaged', 'Expired', 'Lost', 'Correction', 'Other'];

  form = {
    medicineId: null as number | null,
    medicineBatchId: null as number | null,
    adjustmentType: 'Decrease',
    quantity: 1,
    reason: 'Damaged',
    notes: ''
  };

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadAdjustments();
    this.loadMedicines();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadAdjustments() {
    this.loading.set(true);
    this.http.get<StockAdjustment[]>(`${environment.apiUrl}/stock-adjustments/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.adjustments.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadMedicines() {
    this.http.get<Medicine[]>(`${environment.apiUrl}/medicines/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.medicines.set(data));
  }

  get selectedMedicineBatches(): MedicineBatch[] {
    const med = this.medicines().find(m => m.id === this.form.medicineId);
    return med?.batches ?? [];
  }

  onMedicineChange() { this.form.medicineBatchId = null; }

  openAdd() {
    this.form = { medicineId: null, medicineBatchId: null, adjustmentType: 'Decrease', quantity: 1, reason: 'Damaged', notes: '' };
    this.showForm.set(true);
  }

  save() {
    if (!this.form.medicineId || !this.form.medicineBatchId) return;
    this.http.post(`${environment.apiUrl}/stock-adjustments/org/${this.orgId}`, this.form, {
      headers: this.authHeaders()
    }).subscribe(() => {
      this.showForm.set(false);
      this.loadAdjustments();
      this.loadMedicines();
    });
  }
}
