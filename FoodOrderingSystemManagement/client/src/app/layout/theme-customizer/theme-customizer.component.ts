import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  ThemeService, THEMES, CardStyle, SidebarSize, LayoutWidth,
} from '../../core/services/theme.service';

@Component({
  selector: 'app-theme-customizer',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatTooltipModule],
  templateUrl: './theme-customizer.component.html',
  styleUrl: './theme-customizer.component.scss',
})
export class ThemeCustomizerComponent {
  readonly ts     = inject(ThemeService);
  readonly themes = THEMES;

  open = signal(false);

  toggle(): void { this.open.update(v => !v); }
  close(): void  { this.open.set(false); }

  setCardStyle(s: CardStyle):   void { this.ts.setCardStyle(s); }
  setSidebarSize(s: SidebarSize): void { this.ts.setSidebarSize(s); }
  setLayoutWidth(w: LayoutWidth): void { this.ts.setLayoutWidth(w); }
}
