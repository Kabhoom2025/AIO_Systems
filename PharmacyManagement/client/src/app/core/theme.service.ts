import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'pharmacy_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDark = signal(false);

  constructor() {
    const stored = localStorage.getItem(STORAGE_KEY);
    this.setDark(stored === 'dark');
  }

  toggle() {
    this.setDark(!this.isDark());
  }

  private setDark(dark: boolean) {
    this.isDark.set(dark);
    document.documentElement.classList.toggle('dark-mode', dark);
    localStorage.setItem(STORAGE_KEY, dark ? 'dark' : 'light');
  }
}
