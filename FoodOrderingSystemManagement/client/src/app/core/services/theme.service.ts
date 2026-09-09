import { Injectable, signal } from '@angular/core';

export type CardStyle   = 'bordered' | 'borderless' | 'shadow';
export type SidebarSize = 'default'  | 'compact';
export type LayoutWidth = 'fluid'    | 'boxed';

export interface AppTheme {
  id: string;
  name: string;
  primary: string;
  primaryLight: string;
  primaryDark: string;
  accent: string;
  background: string;
  sidebarBg: string;
  sidebarText: string;
}

export interface SidebarPreset { id: string; bg: string; text: string; }
export interface TopBarPreset  { id: string; bg: string; }

export const SIDEBAR_PRESETS: SidebarPreset[] = [
  { id: 'dark',     bg: '#1a1d2e', text: '#e0e0e0' },
  { id: 'charcoal', bg: '#2d2d3a', text: '#e0e0e0' },
  { id: 'navy',     bg: '#0f1b35', text: '#e0e0e0' },
  { id: 'slate',    bg: '#37474f', text: '#e0e0e0' },
  { id: 'blue',     bg: '#1565c0', text: '#ffffff' },
  { id: 'violet',   bg: '#4a148c', text: '#ffffff' },
];

export const TOPBAR_PRESETS: TopBarPreset[] = [
  { id: 'default', bg: '' },
  { id: 'teal',    bg: '#00695c' },
  { id: 'cream',   bg: '#f5f0e8' },
  { id: 'steel',   bg: '#546e7a' },
  { id: 'slate',   bg: '#90a4ae' },
  { id: 'flame',   bg: '#e64a19' },
  { id: 'indigo',  bg: '#3949ab' },
  { id: 'azure',   bg: '#039be5' },
  { id: 'rose',    bg: '#e91e63' },
];

export const THEMES: AppTheme[] = [
  {
    id: 'flame',
    name: 'Flame',
    primary: '#bf360c', primaryLight: '#ff6d40', primaryDark: '#870000',
    accent: '#ffd740', background: '#f5f5f5',
    sidebarBg: '#1a1a2e', sidebarText: '#e0e0e0',
  },
  {
    id: 'ocean',
    name: 'Ocean',
    primary: '#1565c0', primaryLight: '#5e92f3', primaryDark: '#003c8f',
    accent: '#ffd740', background: '#f5f5f5',
    sidebarBg: '#0d1b2a', sidebarText: '#e0e0e0',
  },
  {
    id: 'forest',
    name: 'Forest',
    primary: '#2e7d32', primaryLight: '#60ad5e', primaryDark: '#005005',
    accent: '#ffcc02', background: '#f5f5f5',
    sidebarBg: '#1b2e1b', sidebarText: '#e0e0e0',
  },
  {
    id: 'violet',
    name: 'Violet',
    primary: '#6a1b9a', primaryLight: '#9c4dcc', primaryDark: '#38006b',
    accent: '#ffd740', background: '#f5f5f5',
    sidebarBg: '#1a0a2e', sidebarText: '#e0e0e0',
  },
  {
    id: 'teal',
    name: 'Teal',
    primary: '#00695c', primaryLight: '#439889', primaryDark: '#003d33',
    accent: '#ffd740', background: '#f5f5f5',
    sidebarBg: '#0a1e1c', sidebarText: '#e0e0e0',
  },
  {
    id: 'rose',
    name: 'Rose',
    primary: '#ad1457', primaryLight: '#e35183', primaryDark: '#78002e',
    accent: '#ffd740', background: '#f5f5f5',
    sidebarBg: '#2a0a18', sidebarText: '#e0e0e0',
  },
  {
    id: 'midnight',
    name: 'Midnight',
    primary: '#283593', primaryLight: '#5f5fc4', primaryDark: '#001064',
    accent: '#ffd740', background: '#f0f2f5',
    sidebarBg: '#0a0e2a', sidebarText: '#e0e0e0',
  },
  {
    id: 'slate',
    name: 'Slate',
    primary: '#37474f', primaryLight: '#62727b', primaryDark: '#102027',
    accent: '#ffd740', background: '#f5f5f5',
    sidebarBg: '#102027', sidebarText: '#e0e0e0',
  },
];

const STORAGE_KEY        = 'pos_theme';
const DARK_STORAGE_KEY   = 'pos_dark_mode';
const DRAG_STORAGE_KEY   = 'pos_sidebar_drag';
const CARD_STYLE_KEY     = 'pos_card_style';
const SIDEBAR_SIZE_KEY   = 'pos_sidebar_size';
const LAYOUT_WIDTH_KEY   = 'pos_layout_width';
const TOPBAR_COLOR_KEY   = 'pos_topbar_color';
const SIDEBAR_COLOR_KEY  = 'pos_sidebar_color';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly themes             = THEMES;
  readonly sidebarPresets     = SIDEBAR_PRESETS;
  readonly topBarPresets      = TOPBAR_PRESETS;
  readonly activeThemeId      = signal<string>('flame');
  readonly darkMode           = signal<boolean>(false);
  readonly sidebarDragEnabled = signal<boolean>(false);
  readonly cardStyle          = signal<CardStyle>('bordered');
  readonly sidebarSize        = signal<SidebarSize>('default');
  readonly layoutWidth        = signal<LayoutWidth>('fluid');
  readonly topBarColorId      = signal<string>('default');
  readonly sidebarColorId     = signal<string>('dark');

  constructor() {
    const saved = localStorage.getItem(STORAGE_KEY);
    const theme = THEMES.find(t => t.id === saved) ?? THEMES[0];
    this.apply(theme);

    const dark = localStorage.getItem(DARK_STORAGE_KEY) === 'true';
    this.setDarkMode(dark);

    const drag = localStorage.getItem(DRAG_STORAGE_KEY) === 'true';
    this.sidebarDragEnabled.set(drag);

    this.setCardStyle((localStorage.getItem(CARD_STYLE_KEY) as CardStyle) ?? 'bordered');
    this.setSidebarSize((localStorage.getItem(SIDEBAR_SIZE_KEY) as SidebarSize) ?? 'default');
    this.setLayoutWidth((localStorage.getItem(LAYOUT_WIDTH_KEY) as LayoutWidth) ?? 'fluid');
    this.setTopBarColor(localStorage.getItem(TOPBAR_COLOR_KEY) ?? 'default');
    this.setSidebarColor(localStorage.getItem(SIDEBAR_COLOR_KEY) ?? 'dark');
  }

  apply(theme: AppTheme): void {
    const root = document.documentElement;
    root.style.setProperty('--primary',       theme.primary);
    root.style.setProperty('--primary-light', theme.primaryLight);
    root.style.setProperty('--primary-dark',  theme.primaryDark);
    root.style.setProperty('--accent',        theme.accent);
    root.style.setProperty('--background',    theme.background);
    root.style.setProperty('--sidebar-bg',    theme.sidebarBg);
    root.style.setProperty('--sidebar-text',  theme.sidebarText);
    this.activeThemeId.set(theme.id);
    localStorage.setItem(STORAGE_KEY, theme.id);
  }

  active(): AppTheme {
    return THEMES.find(t => t.id === this.activeThemeId()) ?? THEMES[0];
  }

  toggleDarkMode(): void {
    this.setDarkMode(!this.darkMode());
  }

  setDarkMode(dark: boolean): void {
    this.darkMode.set(dark);
    localStorage.setItem(DARK_STORAGE_KEY, String(dark));
    if (dark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

  setSidebarDrag(enabled: boolean): void {
    this.sidebarDragEnabled.set(enabled);
    localStorage.setItem(DRAG_STORAGE_KEY, String(enabled));
  }

  setCardStyle(style: CardStyle): void {
    this.cardStyle.set(style);
    localStorage.setItem(CARD_STYLE_KEY, style);
    document.body.classList.remove('card-bordered', 'card-borderless', 'card-shadow');
    document.body.classList.add(`card-${style}`);
  }

  setSidebarSize(size: SidebarSize): void {
    this.sidebarSize.set(size);
    localStorage.setItem(SIDEBAR_SIZE_KEY, size);
  }

  setLayoutWidth(width: LayoutWidth): void {
    this.layoutWidth.set(width);
    localStorage.setItem(LAYOUT_WIDTH_KEY, width);
    document.body.classList.remove('layout-fluid', 'layout-boxed');
    document.body.classList.add(`layout-${width}`);
  }

  setTopBarColor(id: string): void {
    this.topBarColorId.set(id);
    localStorage.setItem(TOPBAR_COLOR_KEY, id);
    const preset = TOPBAR_PRESETS.find(p => p.id === id);
    const bg = preset?.bg ?? '';
    document.documentElement.style.setProperty('--topbar-bg', bg);
  }

  setSidebarColor(id: string): void {
    this.sidebarColorId.set(id);
    localStorage.setItem(SIDEBAR_COLOR_KEY, id);
    const preset = SIDEBAR_PRESETS.find(p => p.id === id);
    if (preset) {
      document.documentElement.style.setProperty('--sidebar-bg',   preset.bg);
      document.documentElement.style.setProperty('--sidebar-text', preset.text);
    }
  }
}
