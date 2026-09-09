import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';

interface OrderItem {
  medicineName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface Order {
  id: number;
  invoiceNumber: string;
  saleDate: string;
  branchName: string;
  channel: string;
  totalAmount: number;
  paymentMethod: string;
  status: string;
  items: OrderItem[];
}

@Component({
  selector: 'app-patient-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patient-orders.component.html',
  styleUrl: './patient-orders.component.scss'
})
export class PatientOrdersComponent implements OnInit {
  orders = signal<Order[]>([]);
  loading = signal(false);

  constructor(private http: HttpClient, private auth: PatientAuthService) {}

  ngOnInit() {
    this.loading.set(true);
    this.http.get<Order[]>(`${environment.apiUrl}/sales/my`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: data => { this.orders.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
