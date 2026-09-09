import { Component, Input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="spinner-wrap" [style.min-height]="minHeight">
      <mat-spinner [diameter]="diameter" />
      @if (message) { <p class="spinner-msg">{{ message }}</p> }
    </div>
  `,
  styles: [`
    .spinner-wrap {
      display: flex; flex-direction: column;
      align-items: center; justify-content: center;
      gap: 16px; width: 100%;
    }
    .spinner-msg { color: #9e9e9e; font-size: .9rem; margin: 0; }
  `],
})
export class LoadingSpinnerComponent {
  @Input() diameter  = 48;
  @Input() message   = '';
  @Input() minHeight = '200px';
}
