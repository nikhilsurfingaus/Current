import { Injectable, signal } from '@angular/core';

import { ThemePreference } from '../../shared/models';
import { parseThemePreference } from '../../shared/utils/user-preferences.utils';

const THEME_STORAGE_KEY = 'current.theme';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly activeTheme = signal<'light' | 'dark'>('light');

  readonly themePreference = signal<ThemePreference>(ThemePreference.System);

  private systemThemeListenerRegistered = false;

  constructor() {
    this.applyStoredThemeFallback();
  }

  applyThemePreference(themePreference: ThemePreference): void {
    this.themePreference.set(themePreference);
    localStorage.setItem(THEME_STORAGE_KEY, String(themePreference));
    this.applyResolvedTheme(this.resolveTheme(themePreference));
  }

  initializeFromPreference(themePreference: ThemePreference): void {
    this.themePreference.set(themePreference);
    this.applyResolvedTheme(this.resolveTheme(themePreference));
    this.registerSystemThemeListener();
  }

  private registerSystemThemeListener(): void {
    if (this.systemThemeListenerRegistered) {
      return;
    }

    if (typeof window === 'undefined' || !window.matchMedia) {
      return;
    }

    this.systemThemeListenerRegistered = true;

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const onThemeChange = () => {
      if (this.themePreference() === ThemePreference.System) {
        this.applyResolvedTheme(mediaQuery.matches ? 'dark' : 'light');
      }
    };

    mediaQuery.addEventListener('change', onThemeChange);
  }

  private resolveTheme(themePreference: ThemePreference): 'light' | 'dark' {
    if (themePreference === ThemePreference.Dark) {
      return 'dark';
    }

    if (themePreference === ThemePreference.Light) {
      return 'light';
    }

    if (typeof window !== 'undefined' && window.matchMedia) {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    return 'light';
  }

  private applyResolvedTheme(theme: 'light' | 'dark'): void {
    this.activeTheme.set(theme);

    if (typeof document === 'undefined') {
      return;
    }

    document.documentElement.setAttribute('data-theme', theme);
  }

  private applyStoredThemeFallback(): void {
    const storedTheme = localStorage.getItem(THEME_STORAGE_KEY);
    if (storedTheme === null) {
      return;
    }

    const parsedTheme = parseThemePreference(storedTheme);
    this.initializeFromPreference(parsedTheme);
  }
}
