import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FoodItem } from '../../../core/models/food-item.model';
import { Category } from '../../../core/models/category.model';

export interface FoodItemDialogData {
  foodItem?: FoodItem;
  categories: Category[];
}

@Component({
  selector: 'app-food-item-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './food-item-form-dialog.component.html',
  styleUrl: './food-item-form-dialog.component.scss',
})
export class FoodItemFormDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<FoodItemFormDialogComponent>);
  data: FoodItemDialogData = inject(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;
  saving = false;

  ngOnInit(): void {
    this.isEdit = !!this.data?.foodItem;
    const fi = this.data?.foodItem;
    this.form = this.fb.group({
      categoryId:        [fi?.categoryId        ?? null, Validators.required],
      itemName:          [fi?.itemName          ?? '',   [Validators.required, Validators.maxLength(150)]],
      description:       [fi?.description       ?? '',   Validators.maxLength(500)],
      image:             [fi?.image             ?? '',   Validators.maxLength(500)],
      price:             [fi?.price             ?? null, [Validators.required, Validators.min(0.01)]],
      availableQuantity: [fi?.availableQuantity ?? 0,   [Validators.required, Validators.min(0)]],
      isAvailable:       [fi?.isAvailable       ?? true],
    });
  }

  get itemName()          { return this.form.get('itemName')!; }
  get categoryId()        { return this.form.get('categoryId')!; }
  get price()             { return this.form.get('price')!; }
  get availableQuantity() { return this.form.get('availableQuantity')!; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.value);
  }

  cancel(): void { this.dialogRef.close(); }
}
