import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  RfqApiService, RfqRequestDto, RfqVendorQuoteDto, CreateRfqRequestDto, UpdateRfqRequestDto, CreateRfqItemDto
} from '../../../core/rfq-api.service';
import { VendorApiService, VendorDto } from '../../../core/vendor-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

interface ItemForm {
  itemName: string;
  quantity: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-rfqs',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './rfqs.component.html',
  styleUrl: './rfqs.component.scss'
})
export class RfqsComponent implements OnInit {
  rfqs: RfqRequestDto[] = [];
  vendors: VendorDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: RfqRequestDto | null = null;
  saving = false;
  form: { title: string; description: string | null; issueDate: Date | null; responseDeadline: Date | null; ownerId: number | null } = this.emptyForm();
  items: ItemForm[] = [];
  invitedVendorIds: number[] = [];

  showQuoteDialog = false;
  quoteRfq: RfqRequestDto | null = null;
  quoteVendor: RfqVendorQuoteDto | null = null;
  quoteSaving = false;
  quoteForm: { quotedAmount: number | null; notes: string | null } = { quotedAmount: null, notes: null };

  showCloseDialog = false;
  closingRfq: RfqRequestDto | null = null;
  closeSaving = false;
  winningVendorId: number | null = null;

  constructor(
    private api: RfqApiService,
    private vendorApi: VendorApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.vendorApi.getAll().subscribe({ next: rows => (this.vendors = rows), error: () => (this.vendors = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { title: '', description: null, issueDate: new Date(), responseDeadline: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.rfqs = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load RFQs.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.items = [];
    this.invitedVendorIds = [];
    this.showDialog = true;
  }

  openEdit(rfq: RfqRequestDto) {
    this.editing = rfq;
    this.form = {
      title: rfq.title, description: rfq.description,
      issueDate: new Date(rfq.issueDate), responseDeadline: rfq.responseDeadline ? new Date(rfq.responseDeadline) : null,
      ownerId: rfq.ownerId
    };
    this.items = rfq.items
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(i => ({ itemName: i.itemName, quantity: i.quantity, displayOrder: i.displayOrder }));
    this.invitedVendorIds = rfq.quotes.map(q => q.vendorId);
    this.showDialog = true;
  }

  addItem() {
    const nextOrder = this.items.length ? Math.max(...this.items.map(i => i.displayOrder)) + 1 : 1;
    this.items.push({ itemName: '', quantity: null, displayOrder: nextOrder });
  }

  removeItem(index: number) {
    this.items.splice(index, 1);
  }

  toggleVendor(vendorId: number) {
    const idx = this.invitedVendorIds.indexOf(vendorId);
    if (idx >= 0) this.invitedVendorIds.splice(idx, 1);
    else this.invitedVendorIds.push(vendorId);
  }

  isVendorInvited(vendorId: number): boolean {
    return this.invitedVendorIds.includes(vendorId);
  }

  save() {
    if (!this.form.title || !this.form.ownerId) {
      this.notify.warn('Title and owner are required.');
      return;
    }
    if (!this.items.length || this.items.some(i => !i.itemName || i.quantity == null)) {
      this.notify.warn('Every item needs a name and quantity.');
      return;
    }
    if (!this.invitedVendorIds.length) {
      this.notify.warn('Invite at least one vendor to quote.');
      return;
    }
    this.saving = true;
    const itemDtos: CreateRfqItemDto[] = this.items.map(i => ({
      itemName: i.itemName,
      quantity: i.quantity!,
      displayOrder: i.displayOrder
    }));
    const payload = {
      title: this.form.title,
      description: this.form.description,
      issueDate: (this.form.issueDate ?? new Date()).toISOString(),
      responseDeadline: this.form.responseDeadline ? this.form.responseDeadline.toISOString() : null,
      ownerId: this.form.ownerId,
      items: itemDtos,
      invitedVendorIds: this.invitedVendorIds
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateRfqRequestDto)
      : this.api.create(payload as CreateRfqRequestDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`RFQ ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save RFQ.');
      }
    });
  }

  delete(rfq: RfqRequestDto) {
    this.confirm.confirm({
      message: `Delete RFQ "${rfq.rfqNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(rfq.id).subscribe({
          next: () => {
            this.notify.success('RFQ deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete RFQ.')
        });
      }
    });
  }

  send(rfq: RfqRequestDto) {
    this.api.send(rfq.id).subscribe({
      next: () => {
        this.notify.success('RFQ sent.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to send RFQ.')
    });
  }

  cancelRfq(rfq: RfqRequestDto) {
    this.confirm.confirm({
      message: `Cancel RFQ "${rfq.rfqNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(rfq.id).subscribe({
          next: () => {
            this.notify.success('RFQ cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel RFQ.')
        });
      }
    });
  }

  openRecordQuote(rfq: RfqRequestDto, quote: RfqVendorQuoteDto) {
    this.quoteRfq = rfq;
    this.quoteVendor = quote;
    this.quoteForm = { quotedAmount: quote.quotedAmount, notes: quote.notes };
    this.showQuoteDialog = true;
  }

  submitQuote() {
    if (this.quoteForm.quotedAmount == null) {
      this.notify.warn('Quoted amount is required.');
      return;
    }
    this.quoteSaving = true;
    this.api.recordQuote(this.quoteRfq!.id, {
      vendorId: this.quoteVendor!.vendorId,
      quotedAmount: this.quoteForm.quotedAmount,
      notes: this.quoteForm.notes
    }).subscribe({
      next: () => {
        this.quoteSaving = false;
        this.showQuoteDialog = false;
        this.notify.success('Quote recorded.');
        this.load();
      },
      error: err => {
        this.quoteSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to record quote.');
      }
    });
  }

  openClose(rfq: RfqRequestDto) {
    this.closingRfq = rfq;
    this.winningVendorId = null;
    this.showCloseDialog = true;
  }

  submitClose() {
    this.closeSaving = true;
    this.api.close(this.closingRfq!.id, { winningVendorId: this.winningVendorId }).subscribe({
      next: () => {
        this.closeSaving = false;
        this.showCloseDialog = false;
        this.notify.success('RFQ closed.');
        this.load();
      },
      error: err => {
        this.closeSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to close RFQ.');
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Closed') return 'success';
    if (status === 'Cancelled') return 'danger';
    if (status === 'Sent') return 'warn';
    return 'info';
  }
}
