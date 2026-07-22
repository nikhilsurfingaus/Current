import { ThemePreference } from '../enums';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  themePreference: ThemePreference;
  preferredCurrency: string;
  timezone: string;
  locale: string;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateUserProfileRequest {
  firstName: string;
  lastName: string;
}

export interface UpdateUserPreferencesRequest {
  themePreference: ThemePreference;
  preferredCurrency: string;
  timezone: string;
  locale: string;
}
