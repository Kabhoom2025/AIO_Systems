import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule, FileUploadHandlerEvent } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { DocumentApiService, DocumentDto } from '../../core/document-api.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule, FileUploadModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './documents.component.html',
  styleUrl: './documents.component.scss'
})
export class DocumentsComponent implements OnInit {
  documents: DocumentDto[] = [];
  loading = false;
  uploading = false;

  constructor(
    private api: DocumentApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.documents = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load documents.');
      }
    });
  }

  onUpload(event: FileUploadHandlerEvent, fileUpload: any) {
    const file = event.files?.[0];
    if (!file) return;
    this.uploading = true;
    this.api.upload(file).subscribe({
      next: () => {
        this.uploading = false;
        fileUpload.clear();
        this.notify.success('Document uploaded.');
        this.load();
      },
      error: err => {
        this.uploading = false;
        fileUpload.clear();
        this.notify.error(err.error?.message ?? 'Failed to upload document.');
      }
    });
  }

  download(doc: DocumentDto) {
    this.api.download(doc.id).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = doc.fileName;
        anchor.click();
        window.URL.revokeObjectURL(url);
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to download document.')
    });
  }

  delete(doc: DocumentDto) {
    this.confirm.confirm({
      message: `Delete document "${doc.fileName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(doc.id).subscribe({
          next: () => {
            this.notify.success('Document deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete document.')
        });
      }
    });
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
