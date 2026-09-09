import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';
import { PatientCartService } from '../../../core/patient-cart.service';

interface PublicBranch {
  id: number;
  name: string;
}

@Component({
  selector: 'app-patient-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './patient-cart.component.html',
  styleUrl: './patient-cart.component.scss'
})
export class PatientCartComponent implements OnInit {
  branches = signal<PublicBranch[]>([]);
  branchId: number | null = null;
  paymentMethod = 'Cash';
  submitting = signal(false);
  errorMessage = signal('');

  constructor(
    private http: HttpClient,
    private auth: PatientAuthService,
    public cart: PatientCartService,
    private router: Router
  ) {}

  ngOnInit() {
    const orgId = this.auth.getUser()?.organizationId;
    if (!orgId) return;
    this.http.get<PublicBranch[]>(`${environment.apiUrl}/branches/public/org/${orgId}`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe(data => {
      this.branches.set(data);
      this.branchId = data[0]?.id ?? null;
    });
  }

  placeOrder() {
    const orgId = this.auth.getUser()?.organizationId;
    if (!orgId || !this.branchId || this.cart.cartItems().length === 0) {
      this.errorMessage.set('Please select a pickup location.');
      return;
    }
    this.errorMessage.set('');
    this.submitting.set(true);

    const dto = {
      branchId: this.branchId,
      paymentMethod: this.paymentMethod,
      items: this.cart.cartItems().map(i => ({
        medicineId: i.medicineId,
        quantity: i.quantity,
        discountAmount: 0
      }))
    };

    this.http.post(`${environment.apiUrl}/sales/online/org/${orgId}`, dto, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.cart.clear();
        this.router.navigateByUrl('/patient/orders');
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(err?.error?.message || 'Could not place your order. Please try again.');
      }
    });
  }
}
