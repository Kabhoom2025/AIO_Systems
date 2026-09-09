import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PermissionStateService } from './core/services/permission-state.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class AppComponent {
  // Eagerly inject so constructors run before any route component renders.
  // ThemeService applies the saved color theme AND dark mode class to <html>.
  private readonly _permState = inject(PermissionStateService);
  private readonly _theme     = inject(ThemeService);
}
