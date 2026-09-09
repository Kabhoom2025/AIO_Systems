import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { PlatformModule } from '../../core/models/platform-module.model';

export interface PlatformModuleFormResult {
  name: string;
  key: string;
  description: string;
  icon: string;
  color: string;
  sortOrder: number;
  isActive: boolean;
}

@Component({
  selector: 'app-platform-module-form-dialog',
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './platform-module-form-dialog.component.html',
})
export class PlatformModuleFormDialogComponent {
  model: PlatformModuleFormResult;
  isEdit: boolean;

  constructor(
    private ref: MatDialogRef<PlatformModuleFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { module?: PlatformModule }
  ) {
    this.isEdit = !!data?.module;
    const m = data?.module;
    this.model = {
      name: m?.name ?? '',
      key: m?.key ?? '',
      description: m?.description ?? '',
      icon: m?.icon ?? 'extension',
      color: m?.color ?? '#6c757d',
      sortOrder: m?.sortOrder ?? 0,
      isActive: m?.isActive ?? true,
    };
  }

  save(): void {
    if (!this.model.name.trim() || (!this.isEdit && !this.model.key.trim())) return;
    this.ref.close(this.model);
  }

  cancel(): void {
    this.ref.close();
  }
}
