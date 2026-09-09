import { Component, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SettingsService } from './core/services/settings.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />'
})
export class AppComponent {
  constructor(private readonly settingsService: SettingsService) {
    const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)');

    effect(() => {
      const theme = this.settingsService.settings().theme;
      const isDark = theme === 'dark' || (theme === 'system' && (prefersDark?.matches ?? false));
      document.body.classList.toggle('theme-dark', isDark);
    });

    prefersDark?.addEventListener('change', () => {
      const theme = this.settingsService.settings().theme;
      if (theme === 'system') {
        document.body.classList.toggle('theme-dark', prefersDark.matches);
      }
    });
  }
}
