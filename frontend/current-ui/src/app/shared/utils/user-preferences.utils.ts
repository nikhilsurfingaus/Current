import { ThemePreference } from '../models/enums';
import { User } from '../models/users/user.model';

export const DEFAULT_PREFERRED_CURRENCY = 'AUD';
export const DEFAULT_LOCALE = 'en-AU';
export const DEFAULT_TIMEZONE = 'Australia/Sydney';

export function parseThemePreference(value: unknown): ThemePreference {
  if (typeof value === 'number' && !Number.isNaN(value)) {
    if (value === ThemePreference.Light || value === ThemePreference.Dark || value === ThemePreference.System) {
      return value;
    }
  }

  if (typeof value === 'string') {
    const normalizedTheme = value.trim().toLowerCase();

    if (normalizedTheme === 'light' || normalizedTheme === '0') {
      return ThemePreference.Light;
    }

    if (normalizedTheme === 'dark' || normalizedTheme === '1') {
      return ThemePreference.Dark;
    }

    if (normalizedTheme === 'system' || normalizedTheme === '2') {
      return ThemePreference.System;
    }
  }

  return ThemePreference.System;
}

export function normalizeUserResponse(user: User): User {
  return {
    ...user,
    themePreference: parseThemePreference(user.themePreference),
    preferredCurrency: user.preferredCurrency?.trim() || DEFAULT_PREFERRED_CURRENCY,
    timezone: user.timezone?.trim() || DEFAULT_TIMEZONE,
    locale: user.locale?.trim() || DEFAULT_LOCALE,
  };
}

export function getPreferredCurrency(user: User | null | undefined): string {
  return user?.preferredCurrency ?? DEFAULT_PREFERRED_CURRENCY;
}

export function getPreferredLocale(user: User | null | undefined): string {
  return user?.locale ?? DEFAULT_LOCALE;
}

export function getPreferredTimezone(user: User | null | undefined): string {
  return user?.timezone ?? DEFAULT_TIMEZONE;
}
