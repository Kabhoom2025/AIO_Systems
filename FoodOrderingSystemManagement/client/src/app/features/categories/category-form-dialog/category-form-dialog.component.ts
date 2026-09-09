import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Category } from '../../../core/models/category.model';

export interface CategoryDialogData {
  category?: Category;
}

@Component({
  selector: 'app-category-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './category-form-dialog.component.html',
  styleUrl: './category-form-dialog.component.scss',
})
export class CategoryFormDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<CategoryFormDialogComponent>);
  data: CategoryDialogData = inject(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;
  saving = false;

  ngOnInit(): void {
    this.isEdit = !!this.data?.category;
    this.form = this.fb.group({
      categoryName: [this.data?.category?.categoryName ?? '', [Validators.required, Validators.maxLength(100)]],
      description:  [this.data?.category?.description  ?? '', Validators.maxLength(500)],
      image:        [this.data?.category?.image         ?? '', Validators.maxLength(500)],
      displayOrder: [this.data?.category?.displayOrder  ?? 0, [Validators.required, Validators.min(0)]],
      isActive:     [this.data?.category?.isActive      ?? true],
    });
  }

  get categoryName() { return this.form.get('categoryName')!; }
  get description()  { return this.form.get('description')!; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.value);
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
