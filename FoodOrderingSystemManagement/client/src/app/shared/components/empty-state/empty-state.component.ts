import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule, MatButtonModule],
  template: `
    <div class="empty-wrap">
      <mat-icon class="empty-icon">{{ icon }}</mat-icon>
      <h3>{{ title }}</h3>
      <p>{{ subtitle }}</p>
      <ng-content />
    </div>
  `,
  styles: [`
    .empty-wrap {
      display: flex; flex-direction: column; align-items: center;
      justify-content: center; padding: 80px 0; gap: 10px; text-align: center;
    }
    .empty-icon { font-size: 64px; width: 64px; height: 64px; color: #e0e0e0; }
    h3 { font-size: 1.1rem; font-weight: 600; color: #424242; margin: 0; }
    p  { font-size: .875rem; color: #9e9e9e; margin: 0; }
  `],
})
export class EmptyStateComponent {
  @Input() icon     = 'inbox';
  @Input() title    = 'Nothing here yet';
  @Input() subtitle = '';
}
