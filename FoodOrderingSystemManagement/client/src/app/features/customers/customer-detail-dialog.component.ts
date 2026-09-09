import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { CustomerService } from '../../core/services/customer.service';
import { HasPermissionDirective } from '../../core/directives/has-permission.directive';
import {
  Customer, PointsTransaction, SavedAddress,
  AddSavedAddressRequest, CustomerOrderSummary,
} from '../../core/models/customer.model';

@Component({
  selector: 'app-customer-detail-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatButtonModule, MatDialogModule, MatIconModule,
    MatProgressSpinnerModule, MatFormFieldModule, MatInputModule,
    MatTabsModule, MatDividerModule, MatSelectModule, MatTooltipModule,
    MatSnackBarModule, HasPermissionDirective,
  ],
  template: `
    <div class="detail-header" [class]="tierColor(customer.tier)">
      <div class="avatar">{{ customer.name[0] | uppercase }}</div>
      <div class="header-info">
        <h2>{{ customer.name }}</h2>
        <span class="tier-label">
          <mat-icon>{{ tierIcon(customer.tier) }}</mat-icon>
          {{ customer.tier }} Member
        </span>
      </div>
      <button mat-icon-button mat-dialog-close class="close-btn"><mat-icon>close</mat-icon></button>
    </div>

    <mat-dialog-content>
      <mat-tab-group>

        <!-- ── Profile ── -->
        <mat-tab label="Profile">
          <div class="profile-grid">
            <div class="info-row"><mat-icon>phone</mat-icon><span>{{ customer.phone }}</span></div>
            <div class="info-row"><mat-icon>email</mat-icon><span>{{ customer.email || '—' }}</span></div>
            <div class="info-row"><mat-icon>cake</mat-icon><span>{{ customer.birthday ? (customer.birthday | date:'dd MMM yyyy') : '—' }}</span></div>
            <div *hasPermission="'customers.notes'" class="info-row notes-row">
              <mat-icon>notes</mat-icon>
              <span>{{ customer.notes || 'No notes added.' }}</span>
            </div>
          </div>

          <div class="stats-row">
            <div class="stat">
              <span class="val">₹{{ customer.totalSpend | number:'1.2-2' }}</span>
              <span class="lbl">Total Spend</span>
            </div>
            <div class="stat">
              <span class="val">{{ customer.totalVisits }}</span>
              <span class="lbl">Visits</span>
            </div>
            <div *hasPermission="'customers.loyalty'" class="stat highlight">
              <span class="val">{{ customer.loyaltyPoints | number }}</span>
              <span class="lbl">Points</span>
            </div>
          </div>
        </mat-tab>

        <!-- ── Points History ── -->
        <mat-tab label="Points History">
          <ng-template matTabContent>
            <div *hasPermission="'customers.loyalty'">
              @if (loadingTxns()) {
                <div class="tab-loading"><mat-spinner diameter="32" /></div>
              } @else if (transactions().length === 0) {
                <div class="tab-empty"><mat-icon>history</mat-icon><p>No transactions yet.</p></div>
              } @else {
                <div class="timeline">
                  @for (t of transactions(); track t.id) {
                    <div class="timeline-item" [class.earn]="t.type === 'Earn'" [class.redeem]="t.type === 'Redeem'" [class.adjust]="t.type === 'Adjust'">
                      <div class="timeline-dot">
                        <mat-icon>{{ t.type === 'Earn' ? 'add_circle' : t.type === 'Redeem' ? 'remove_circle' : 'tune' }}</mat-icon>
                      </div>
                      <div class="timeline-content">
                        <div class="txn-desc">{{ t.description }}</div>
                        <div class="txn-meta">
                          <span class="txn-points" [class.positive]="t.points > 0" [class.negative]="t.points < 0">
                            {{ t.points > 0 ? '+' : '' }}{{ t.points }} pts
                          </span>
                          <span class="txn-date">{{ t.createdDate | date:'dd MMM yyyy, h:mm a' }}</span>
                        </div>
                      </div>
                    </div>
                  }
                </div>
              }
            </div>
          </ng-template>
        </mat-tab>

        <!-- ── Wallet / Adjust Points ── -->
        <mat-tab label="Wallet">
          <ng-template matTabContent>
            <div *hasPermission="'customers.wallet'" class="wallet-section">
              <div class="wallet-balance">
                <mat-icon>account_balance_wallet</mat-icon>
                <div>
                  <span class="balance-val">{{ customer.loyaltyPoints | number }}</span>
                  <span class="balance-lbl">Current Points Balance</span>
                </div>
              </div>
              <form [formGroup]="adjustForm" class="adjust-form">
                <mat-form-field appearance="outline" class="full">
                  <mat-label>Points (+/−)</mat-label>
                  <input matInput type="number" formControlName="points" />
                  <mat-hint>Use positive to add, negative to deduct.</mat-hint>
                </mat-form-field>
                <mat-form-field appearance="outline" class="full">
                  <mat-label>Reason *</mat-label>
                  <input matInput formControlName="description" />
                </mat-form-field>
                <button mat-raised-button color="accent" [disabled]="adjustForm.invalid || adjusting()" (click)="adjustPoints()">
                  @if (adjusting()) { <mat-spinner diameter="18" /> } @else { Apply Adjustment }
                </button>
              </form>
            </div>
          </ng-template>
        </mat-tab>

        <!-- ── Purchase History ── -->
        <mat-tab label="Purchase History">
          <ng-template matTabContent>
            <div *hasPermission="'customers.history'">
              @if (loadingOrders()) {
                <div class="tab-loading"><mat-spinner diameter="32" /></div>
              } @else if (orders().length === 0) {
                <div class="tab-empty"><mat-icon>receipt_long</mat-icon><p>No orders found.</p></div>
              } @else {
                <div class="orders-list">
                  @for (o of orders(); track o.id) {
                    <div class="order-row">
                      <div class="order-num">{{ o.orderNumber }}</div>
                      <div class="order-date">{{ o.orderDate | date:'dd MMM yyyy, h:mm a' }}</div>
                      <span class="order-status" [class]="o.status.toLowerCase()">{{ o.status }}</span>
                      <div class="order-total">₹{{ o.grandTotal | number:'1.2-2' }}</div>
                      @if (o.pointsEarned > 0) {
                        <span class="pts-chip earned">+{{ o.pointsEarned }} pts</span>
                      }
                      @if (o.pointsRedeemed > 0) {
                        <span class="pts-chip redeemed">−{{ o.pointsRedeemed }} pts</span>
                      }
                    </div>
                  }
                </div>
              }
            </div>
          </ng-template>
        </mat-tab>

        <!-- ── Saved Addresses ── -->
        <mat-tab label="Addresses">
          <ng-template matTabContent>
            <div *hasPermission="'customers.addresses'" class="addresses-section">
              @if (loadingAddresses()) {
                <div class="tab-loading"><mat-spinner diameter="32" /></div>
              } @else {
                <div class="addresses-list">
                  @for (a of addresses(); track a.id) {
                    <div class="address-card" [class.default]="a.isDefault">
                      <div class="addr-label">
                        <mat-icon>{{ a.label === 'Home' ? 'home' : a.label === 'Work' ? 'business' : 'place' }}</mat-icon>
                        {{ a.label }}
                        @if (a.isDefault) { <span class="default-badge">Default</span> }
                      </div>
                      <div class="addr-line">{{ a.addressLine }}</div>
                      @if (a.city) { <div class="addr-city">{{ a.city }}</div> }
                      <button mat-icon-button color="warn" class="addr-delete"
                              (click)="deleteAddress(a)" matTooltip="Remove address">
                        <mat-icon>delete</mat-icon>
                      </button>
                    </div>
                  }
                  @if (addresses().length === 0) {
                    <div class="tab-empty"><mat-icon>location_off</mat-icon><p>No saved addresses.</p></div>
                  }
                </div>

                <!-- Add address form -->
                <div class="add-address-form">
                  <h4>Add Address</h4>
                  <mat-form-field appearance="outline">
                    <mat-label>Label</mat-label>
                    <mat-select [(ngModel)]="newAddr.label">
                      <mat-option value="Home">Home</mat-option>
                      <mat-option value="Work">Work</mat-option>
                      <mat-option value="Other">Other</mat-option>
                    </mat-select>
                  </mat-form-field>
                  <mat-form-field appearance="outline" class="full">
                    <mat-label>Address Line *</mat-label>
                    <input matInput [(ngModel)]="newAddr.addressLine" />
                  </mat-form-field>
                  <mat-form-field appearance="outline">
                    <mat-label>City</mat-label>
                    <input matInput [(ngModel)]="newAddr.city" />
                  </mat-form-field>
                  <label class="default-check">
                    <input type="checkbox" [(ngModel)]="newAddr.isDefault" />
                    Set as default
                  </label>
                  <button mat-raised-button color="primary"
                          [disabled]="!newAddr.addressLine || savingAddress()"
                          (click)="saveAddress()">
                    @if (savingAddress()) { <mat-spinner diameter="18" /> } @else { Save Address }
                  </button>
                </div>
              }
            </div>
          </ng-template>
        </mat-tab>

      </mat-tab-group>
    </mat-dialog-content>
  `,
  styles: [`
    .detail-header {
      display: flex; align-items: center; gap: 16px;
      padding: 20px 24px 16px; position: relative;

      &.gold   { background: linear-gradient(135deg, #fff8e1, #fffde7); }
      &.silver { background: linear-gradient(135deg, #f5f5f5, #fafafa); }
      &.bronze { background: linear-gradient(135deg, #fdf3e7, #fefefe); }

      .avatar {
        width: 52px; height: 52px; border-radius: 50%;
        background: #1976d2; color: white;
        display: flex; align-items: center; justify-content: center;
        font-size: 22px; font-weight: 700;
      }
      .header-info h2 { margin: 0 0 4px; font-size: 20px; }
      .tier-label {
        display: flex; align-items: center; gap: 4px;
        font-size: 13px; color: #888;
        mat-icon { font-size: 16px; width: 16px; height: 16px; }
      }
      .close-btn { position: absolute; top: 12px; right: 12px; }
    }

    mat-dialog-content { padding: 0 !important; min-width: 560px; }

    .profile-grid {
      padding: 16px 24px;
      .info-row { display: flex; align-items: flex-start; gap: 12px; padding: 8px 0; color: #444;
        mat-icon { color: #9e9e9e; font-size: 18px; width: 18px; min-width: 18px; } }
      .notes-row span { font-style: italic; color: #666; }
    }

    .stats-row {
      display: flex; border-top: 1px solid #eee; border-bottom: 1px solid #eee; margin: 8px 0;
      .stat {
        flex: 1; padding: 16px; text-align: center; border-right: 1px solid #eee;
        &:last-child { border-right: none; }
        &.highlight .val { color: #2e7d32; }
        .val { display: block; font-size: 20px; font-weight: 700; }
        .lbl { display: block; font-size: 12px; color: #888; margin-top: 2px; }
      }
    }

    .tab-loading, .tab-empty { display: flex; flex-direction: column; align-items: center; padding: 32px; gap: 12px; color: #888; }

    /* Points history */
    .timeline { padding: 16px 24px; display: flex; flex-direction: column; gap: 12px; max-height: 340px; overflow-y: auto; }
    .timeline-item {
      display: flex; align-items: flex-start; gap: 12px; padding: 10px; border-radius: 8px;
      &.earn   { background: #e8f5e9; }
      &.redeem { background: #fff3e0; }
      &.adjust { background: #f3e5f5; }
    }
    .timeline-dot mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .txn-desc { font-size: 14px; font-weight: 500; }
    .txn-meta { display: flex; gap: 12px; margin-top: 2px; align-items: center; }
    .txn-points { font-weight: 700; font-size: 13px; &.positive { color: #2e7d32; } &.negative { color: #c62828; } }
    .txn-date { font-size: 12px; color: #888; }

    /* Wallet */
    .wallet-section { padding: 20px 24px; }
    .wallet-balance {
      display: flex; align-items: center; gap: 16px; padding: 16px;
      background: linear-gradient(135deg, #e8f5e9, #f1f8e9); border-radius: 12px; margin-bottom: 20px;
      mat-icon { font-size: 36px; width: 36px; height: 36px; color: #2e7d32; }
      .balance-val { display: block; font-size: 28px; font-weight: 700; color: #1b5e20; }
      .balance-lbl { display: block; font-size: 13px; color: #558b2f; }
    }
    .adjust-form { display: flex; flex-direction: column; gap: 12px; .full { width: 100%; } }

    /* Purchase history */
    .orders-list { padding: 8px 24px; max-height: 360px; overflow-y: auto; display: flex; flex-direction: column; gap: 8px; }
    .order-row {
      display: flex; align-items: center; gap: 12px; padding: 10px 12px;
      border: 1px solid #eee; border-radius: 8px; font-size: 13px;
      .order-num { font-weight: 600; min-width: 80px; }
      .order-date { color: #888; flex: 1; }
      .order-total { font-weight: 700; }
      .order-status {
        padding: 2px 8px; border-radius: 20px; font-size: 11px; font-weight: 600; text-transform: uppercase;
        &.completed { background: #e8f5e9; color: #2e7d32; }
        &.cancelled  { background: #ffebee; color: #c62828; }
        &.pending    { background: #fff3e0; color: #e65100; }
        &.billpending { background: #e3f2fd; color: #1565c0; }
        &.kitchenready { background: #f3e5f5; color: #7b1fa2; }
      }
      .pts-chip {
        padding: 2px 6px; border-radius: 10px; font-size: 11px; font-weight: 600;
        &.earned   { background: #e8f5e9; color: #2e7d32; }
        &.redeemed { background: #fff3e0; color: #e65100; }
      }
    }

    /* Addresses */
    .addresses-section { padding: 16px 24px; }
    .addresses-list { display: flex; flex-direction: column; gap: 8px; margin-bottom: 20px; max-height: 200px; overflow-y: auto; }
    .address-card {
      display: flex; flex-wrap: wrap; gap: 4px; align-items: center;
      padding: 10px 12px; border: 1px solid #e0e0e0; border-radius: 8px; position: relative;
      &.default { border-color: #1976d2; background: #e3f2fd; }
      .addr-label { display: flex; align-items: center; gap: 6px; font-weight: 600; font-size: 13px; width: 100%; }
      .default-badge { background: #1976d2; color: white; font-size: 10px; padding: 1px 6px; border-radius: 10px; margin-left: 4px; }
      .addr-line { font-size: 13px; color: #333; width: 100%; }
      .addr-city { font-size: 12px; color: #888; width: 100%; }
      .addr-delete { position: absolute; top: 4px; right: 4px; }
    }
    .add-address-form {
      border-top: 1px solid #eee; padding-top: 16px;
      display: flex; flex-wrap: wrap; gap: 10px; align-items: flex-start;
      h4 { width: 100%; margin: 0 0 4px; font-size: 14px; color: #555; }
      mat-form-field { flex: 1; min-width: 140px; }
      .full { flex: 100%; }
      .default-check { display: flex; align-items: center; gap: 6px; font-size: 13px; align-self: center; }
    }
  `],
})
export class CustomerDetailDialogComponent implements OnInit {
  private readonly service = inject(CustomerService);
  private readonly snack   = inject(MatSnackBar);
  private readonly fb      = inject(FormBuilder);
  readonly customer: Customer = inject(MAT_DIALOG_DATA);

  loadingTxns      = signal(false);
  loadingOrders    = signal(false);
  loadingAddresses = signal(false);
  adjusting        = signal(false);
  savingAddress    = signal(false);

  transactions = signal<PointsTransaction[]>([]);
  orders       = signal<CustomerOrderSummary[]>([]);
  addresses    = signal<SavedAddress[]>([]);

  adjustForm!: FormGroup;
  newAddr: AddSavedAddressRequest = { label: 'Home', addressLine: '', city: '', isDefault: false };

  ngOnInit() {
    this.adjustForm = this.fb.group({
      points:      [null, [Validators.required, Validators.min(-9999), Validators.max(9999)]],
      description: ['', Validators.required],
    });
    this.loadTransactions();
    this.loadOrders();
    this.loadAddresses();
  }

  loadTransactions() {
    this.loadingTxns.set(true);
    this.service.getTransactions(this.customer.id).subscribe({
      next: res => { this.transactions.set(res.data ?? []); this.loadingTxns.set(false); },
      error: () => this.loadingTxns.set(false),
    });
  }

  loadOrders() {
    this.loadingOrders.set(true);
    this.service.getOrders(this.customer.id).subscribe({
      next: res => { this.orders.set(res.data ?? []); this.loadingOrders.set(false); },
      error: () => this.loadingOrders.set(false),
    });
  }

  loadAddresses() {
    this.loadingAddresses.set(true);
    this.service.getAddresses(this.customer.id).subscribe({
      next: res => { this.addresses.set(res.data ?? []); this.loadingAddresses.set(false); },
      error: () => this.loadingAddresses.set(false),
    });
  }

  adjustPoints() {
    if (this.adjustForm.invalid) return;
    this.adjusting.set(true);
    const v = this.adjustForm.value;
    this.service.adjustPoints(this.customer.id, { points: v.points, description: v.description }).subscribe({
      next: res => {
        this.adjusting.set(false);
        if (res.data) this.customer.loyaltyPoints = res.data.loyaltyPoints;
        this.adjustForm.reset();
        this.loadTransactions();
        this.snack.open('Points adjusted.', 'OK', { duration: 3000 });
      },
      error: () => { this.adjusting.set(false); this.snack.open('Adjustment failed.', 'OK', { duration: 3000 }); },
    });
  }

  saveAddress() {
    if (!this.newAddr.addressLine) return;
    this.savingAddress.set(true);
    this.service.addAddress(this.customer.id, this.newAddr).subscribe({
      next: () => {
        this.savingAddress.set(false);
        this.newAddr = { label: 'Home', addressLine: '', city: '', isDefault: false };
        this.loadAddresses();
        this.snack.open('Address saved.', 'OK', { duration: 2500 });
      },
      error: () => { this.savingAddress.set(false); this.snack.open('Failed to save address.', 'OK', { duration: 2500 }); },
    });
  }

  deleteAddress(address: SavedAddress) {
    this.service.deleteAddress(this.customer.id, address.id).subscribe({
      next: () => {
        this.loadAddresses();
        this.snack.open('Address removed.', 'OK', { duration: 2500 });
      },
      error: () => this.snack.open('Failed to remove address.', 'OK', { duration: 2500 }),
    });
  }

  tierColor(tier: string): string {
    return tier === 'Gold' ? 'gold' : tier === 'Silver' ? 'silver' : 'bronze';
  }

  tierIcon(tier: string): string {
    return tier === 'Gold' ? 'emoji_events' : tier === 'Silver' ? 'workspace_premium' : 'military_tech';
  }
}
