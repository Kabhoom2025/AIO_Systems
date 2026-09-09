import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { environment } from '../../../environments/environment';

interface PrescriptionItem { id: number; medicineName: string; dosage: string; duration: string; quantity: number; isDispensed: boolean; }
interface Prescription { id: number; patientName: string; patientPhone?: string; patientAge?: number; doctorName: string; doctorRegNo?: string; hospitalName?: string; prescriptionDate: string; status: string; notes?: string; createdDate: string; items: PrescriptionItem[]; }

@Component({
  selector: 'app-prescriptions',
  standalone: true,
  imports: [FormsModule, DatePipe],
  template: `
    <div class="prescriptions-page">
      <div class="page-header">
        <h2>Prescriptions</h2>
        <button class="btn-primary" (click)="openAdd()">+ New Prescription</button>
      </div>

      <div class="prescriptions-list">
        @for (p of prescriptions(); track p.id) {
          <div class="prescription-card" (click)="selectedId.set(p.id === selectedId() ? null : p.id)">
            <div class="card-top">
              <div class="patient-info">
                <h4>{{ p.patientName }}</h4>
                <span>Dr. {{ p.doctorName }}</span>
              </div>
              <div class="right-info">
                <span class="status-badge" [class]="'status-' + p.status.toLowerCase()">{{ p.status }}</span>
                <span class="date">{{ p.createdDate | date:'dd MMM yy' }}</span>
              </div>
            </div>
            @if (selectedId() === p.id) {
              <div class="items-list">
                @for (item of p.items; track item.id) {
                  <div class="item-row" [class.dispensed]="item.isDispensed">
                    <span class="med-name">{{ item.medicineName }}</span>
                    <span class="dosage">{{ item.dosage }}</span>
                    <span class="duration">{{ item.duration }}</span>
                    <span class="qty">Qty: {{ item.quantity }}</span>
                    @if (!item.isDispensed) {
                      <button class="dispense-btn" (click)="dispense(p.id, item.id, $event)">Dispense</button>
                    } @else {
                      <span class="dispensed-label">✓ Dispensed</span>
                    }
                  </div>
                }
              </div>
            }
          </div>
        }
      </div>

      @if (showForm()) {
        <div class="dialog-overlay">
          <div class="dialog">
            <h3>New Prescription</h3>
            <div class="form-grid">
              <label>Patient Name <input [(ngModel)]="form.patientName" /></label>
              <label>Phone <input [(ngModel)]="form.patientPhone" /></label>
              <label>Age <input type="number" [(ngModel)]="form.patientAge" /></label>
              <label>Doctor Name <input [(ngModel)]="form.doctorName" /></label>
              <label>Doctor Reg No <input [(ngModel)]="form.doctorRegNo" /></label>
              <label>Hospital <input [(ngModel)]="form.hospitalName" /></label>
            </div>
            <h4>Medicines</h4>
            @for (item of form.items; track $index; let i = $index) {
              <div class="item-form-row">
                <input [(ngModel)]="item.medicineName" placeholder="Medicine name" />
                <input [(ngModel)]="item.dosage" placeholder="Dosage (1-0-1)" />
                <input [(ngModel)]="item.duration" placeholder="5 days" />
                <input type="number" [(ngModel)]="item.quantity" placeholder="Qty" />
                <button (click)="removeItem(i)">✕</button>
              </div>
            }
            <button class="add-item-btn" (click)="addItem()">+ Add Medicine</button>
            <div class="dialog-actions">
              <button (click)="showForm.set(false)">Cancel</button>
              <button class="btn-primary" (click)="save()">Save</button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .prescriptions-page { padding: 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;
      h2 { margin: 0; color: #1a237e; } }
    .btn-primary { background: #1a237e; color: #fff; border: none; padding: 0.6rem 1.2rem; border-radius: 6px; cursor: pointer; }
    .prescriptions-list { display: flex; flex-direction: column; gap: 0.75rem; }
    .prescription-card { border: 1px solid #e0e0e0; border-radius: 10px; overflow: hidden; background: #fff; cursor: pointer; }
    .card-top { display: flex; justify-content: space-between; padding: 1rem; }
    .patient-info h4 { margin: 0; color: #1a237e; }
    .patient-info span { font-size: 0.85rem; color: #666; }
    .right-info { display: flex; flex-direction: column; align-items: flex-end; gap: 4px; }
    .status-badge { padding: 3px 10px; border-radius: 12px; font-size: 0.75rem; font-weight: 700;
      &.status-pending { background: #fff3e0; color: #e65100; }
      &.status-partial { background: #e3f2fd; color: #0d47a1; }
      &.status-dispensed { background: #e8f5e9; color: #2e7d32; }
    }
    .date { font-size: 0.8rem; color: #999; }
    .items-list { border-top: 1px solid #f0f0f0; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem; }
    .item-row { display: flex; gap: 1rem; align-items: center; font-size: 0.85rem; padding: 6px 0;
      &.dispensed { opacity: 0.5; text-decoration: line-through; }
      .med-name { font-weight: 600; min-width: 150px; }
      .dosage, .duration { color: #666; }
      .qty { color: #333; }
    }
    .dispense-btn { margin-left: auto; background: #1a237e; color: #fff; border: none; padding: 4px 12px; border-radius: 4px; cursor: pointer; font-size: 0.8rem; }
    .dispensed-label { margin-left: auto; color: #43a047; font-size: 0.8rem; }
    .dialog-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .dialog { background: #fff; border-radius: 12px; padding: 2rem; width: 700px; max-width: 95vw; max-height: 90vh; overflow-y: auto;
      h3, h4 { color: #1a237e; } }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1.5rem;
      label { display: flex; flex-direction: column; font-size: 0.85rem; gap: 4px;
        input { padding: 8px; border: 1px solid #ddd; border-radius: 6px; } }
    }
    .item-form-row { display: grid; grid-template-columns: 2fr 1fr 1fr 80px 30px; gap: 8px; margin-bottom: 8px;
      input { padding: 6px; border: 1px solid #ddd; border-radius: 4px; }
      button { background: #e53935; color: #fff; border: none; border-radius: 4px; cursor: pointer; }
    }
    .add-item-btn { background: none; border: 1px dashed #1a237e; color: #1a237e; padding: 6px 16px; border-radius: 6px; cursor: pointer; margin: 0.5rem 0; }
    .dialog-actions { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem;
      button { padding: 8px 20px; border-radius: 6px; cursor: pointer; border: 1px solid #ddd; }
    }
  `]
})
export class PrescriptionsComponent implements OnInit {
  prescriptions = signal<Prescription[]>([]);
  showForm      = signal(false);
  selectedId    = signal<number | null>(null);
  orgId         = 1;

  form = { patientName: '', patientPhone: '', patientAge: 0, doctorName: '', doctorRegNo: '', hospitalName: '',
    items: [{ medicineName: '', dosage: '', duration: '', quantity: 1 }] };

  constructor(private http: HttpClient) {}

  ngOnInit() { this.load(); }

  load() {
    this.http.get<Prescription[]>(`${environment.apiUrl}/prescriptions/org/${this.orgId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe(data => this.prescriptions.set(data));
  }

  openAdd() {
    this.form = { patientName: '', patientPhone: '', patientAge: 0, doctorName: '', doctorRegNo: '', hospitalName: '',
      items: [{ medicineName: '', dosage: '', duration: '', quantity: 1 }] };
    this.showForm.set(true);
  }

  addItem() { this.form.items.push({ medicineName: '', dosage: '', duration: '', quantity: 1 }); }
  removeItem(i: number) { this.form.items.splice(i, 1); }

  save() {
    this.http.post(`${environment.apiUrl}/prescriptions/org/${this.orgId}`, this.form, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe(() => { this.showForm.set(false); this.load(); });
  }

  dispense(pId: number, iId: number, e: Event) {
    e.stopPropagation();
    this.http.patch(`${environment.apiUrl}/prescriptions/${pId}/items/${iId}/dispense`, {}, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe(() => this.load());
  }
}
