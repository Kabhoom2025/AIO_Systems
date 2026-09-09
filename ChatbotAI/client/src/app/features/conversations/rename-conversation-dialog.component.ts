import { Component, Inject, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-rename-conversation-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <h2 mat-dialog-title>Rename conversation</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="rename-field">
        <mat-label>Title</mat-label>
        <input matInput [(ngModel)]="title" (keydown.enter)="dialogRef.close(title)" cdkFocusInitial />
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-flat-button color="primary" (click)="dialogRef.close(title)">Save</button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .rename-field {
        width: 100%;
        min-width: 320px;
      }
    `
  ]
})
export class RenameConversationDialogComponent {
  readonly dialogRef = inject(MatDialogRef<RenameConversationDialogComponent, string>);

  title: string;

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: { title: string }) {
    this.title = data.title;
  }
}
