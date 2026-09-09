import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import JsBarcode from 'jsbarcode';
import * as QRCode from 'qrcode';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getAuthHeaders } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { exportToCsv, parseCsv } from '../../core/csv.util';

interface Medicine {
  id: number; name: string; genericName: string; category: string;
  drugSchedule: string; packType: string; packSize: string; unit: string;
  manufacturer: string; mrp: number; purchasePrice: number; gstPercent: number;
  reorderLevel: number; isActive: boolean; totalStock: number;
  rackLocation?: string; batches: any[]; sku: string; barcode: string;
}

@Component({
  selector: 'app-medicines',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './medicines.component.html',
  styleUrl: './medicines.component.scss'
})
export class MedicinesComponent implements OnInit {
  medicines = signal<Medicine[]>([]);
  loading   = signal(false);
  showForm  = signal(false);
  editId    = signal<number | null>(null);
  labelMedicine = signal<Medicine | null>(null);

  orgId = getOrgIdFromToken();

  form = {
    name: '', genericName: '', category: '', drugSchedule: 'OTC',
    packType: '', packSize: '', unit: '', manufacturer: '',
    mrp: 0, purchasePrice: 0, gstPercent: 12, reorderLevel: 10,
    rackLocation: '', isActive: true
  };

  scheduleOptions = ['OTC', 'H', 'H1', 'X'];
  categoryOptions = ['Tablets', 'Capsules', 'Syrup', 'Injection', 'Drops', 'Ointment', 'Others'];

  constructor(private http: HttpClient, private notify: NotificationService) {}

  ngOnInit() { this.loadMedicines(); }

  loadMedicines() {
    this.loading.set(true);
    this.http.get<Medicine[]>(`${environment.apiUrl}/medicines/org/${this.orgId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe({
      next: data => { this.medicines.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openAdd() {
    this.editId.set(null);
    this.form = { name: '', genericName: '', category: '', drugSchedule: 'OTC',
      packType: '', packSize: '', unit: '', manufacturer: '',
      mrp: 0, purchasePrice: 0, gstPercent: 12, reorderLevel: 10, rackLocation: '', isActive: true };
    this.showForm.set(true);
  }

  openEdit(m: Medicine) {
    this.editId.set(m.id);
    this.form = { name: m.name, genericName: m.genericName, category: m.category,
      drugSchedule: m.drugSchedule, packType: m.packType, packSize: m.packSize,
      unit: m.unit, manufacturer: m.manufacturer, mrp: m.mrp,
      purchasePrice: m.purchasePrice, gstPercent: m.gstPercent,
      reorderLevel: m.reorderLevel, rackLocation: m.rackLocation ?? '', isActive: m.isActive };
    this.showForm.set(true);
  }

  save() {
    const token = localStorage.getItem(environment.tokenKey);
    const headers = { Authorization: `Bearer ${token}` };
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/medicines/${id}`, this.form, { headers })
      : this.http.post(`${environment.apiUrl}/medicines/org/${this.orgId}`, this.form, { headers });
    req$.subscribe(() => { this.showForm.set(false); this.loadMedicines(); });
  }

  delete(id: number) {
    if (!confirm('Delete this medicine?')) return;
    this.http.delete(`${environment.apiUrl}/medicines/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe(() => this.loadMedicines());
  }

  scheduleColor(s: string) {
    return ({ OTC: '#43a047', H: '#fb8c00', H1: '#e53935', X: '#8e24aa' } as any)[s] ?? '#757575';
  }

  openLabel(m: Medicine) {
    this.labelMedicine.set(m);
    setTimeout(() => this.renderLabel(m), 0);
  }

  closeLabel() {
    this.labelMedicine.set(null);
  }

  printLabel() {
    window.print();
  }

  exportCsv() {
    exportToCsv('medicines.csv', this.medicines().map(m => ({
      name: m.name, genericName: m.genericName, category: m.category,
      sku: m.sku, barcode: m.barcode, mrp: m.mrp, purchasePrice: m.purchasePrice,
      totalStock: m.totalStock, reorderLevel: m.reorderLevel
    })));
  }

  onImportFile(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = async () => {
      const rows = parseCsv(String(reader.result));
      let succeeded = 0;
      for (const row of rows) {
        const dto = {
          name: row['name'] ?? '',
          genericName: row['genericName'] ?? '',
          category: row['category'] ?? '',
          drugSchedule: row['drugSchedule'] || 'OTC',
          packType: row['packType'] ?? '',
          packSize: row['packSize'] ?? '',
          unit: row['unit'] ?? '',
          manufacturer: row['manufacturer'] ?? '',
          mrp: Number(row['mrp']) || 0,
          purchasePrice: Number(row['purchasePrice']) || 0,
          gstPercent: Number(row['gstPercent']) || 12,
          reorderLevel: Number(row['reorderLevel']) || 10
        };
        try {
          await firstValueFrom(this.http.post(`${environment.apiUrl}/medicines/org/${this.orgId}`, dto, {
            headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
          }));
          succeeded++;
        } catch {
          // continue importing remaining rows; failures are reflected in the summary count
        }
      }
      input.value = '';
      this.notify.success(`Imported ${succeeded} of ${rows.length} medicines.`);
      this.loadMedicines();
    };
    reader.readAsText(file);
  }

  private renderLabel(m: Medicine) {
    const barcodeEl = document.getElementById('label-barcode');
    if (barcodeEl) {
      JsBarcode(barcodeEl, m.barcode, { format: 'CODE128', width: 2, height: 50, displayValue: true, fontSize: 14 });
    }
    const qrEl = document.getElementById('label-qr') as HTMLCanvasElement | null;
    if (qrEl) {
      QRCode.toCanvas(qrEl, m.barcode, { width: 100 });
    }
  }
}
