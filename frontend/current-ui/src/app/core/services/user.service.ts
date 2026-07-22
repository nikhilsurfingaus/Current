import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ThemeService } from './theme.service';
import { ApiService } from './api.service';
import {
  UpdateUserPreferencesRequest,
  UpdateUserProfileRequest,
  User,
} from '../../shared/models';
import { normalizeUserResponse } from '../../shared/utils/user-preferences.utils';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private currentUserState = signal<User | null>(null);

  readonly currentUser = this.currentUserState.asReadonly();

  constructor(
    private apiService: ApiService,
    private themeService: ThemeService,
  ) {}

  loadCurrentUser(): Observable<User> {
    return this.apiService.get<User>(API_PATHS.users.me).pipe(
      tap((user) => {
        const normalizedUser = normalizeUserResponse(user);
        this.currentUserState.set(normalizedUser);
        this.themeService.initializeFromPreference(normalizedUser.themePreference);
      }),
    );
  }

  updateProfile(request: UpdateUserProfileRequest): Observable<User> {
    return this.apiService.put<User>(API_PATHS.users.profile, request).pipe(
      tap((user) => this.currentUserState.set(normalizeUserResponse(user))),
    );
  }

  updatePreferences(request: UpdateUserPreferencesRequest): Observable<User> {
    return this.apiService.put<User>(API_PATHS.users.preferences, request).pipe(
      tap((user) => {
        const normalizedUser = normalizeUserResponse(user);
        this.currentUserState.set(normalizedUser);
        this.themeService.applyThemePreference(normalizedUser.themePreference);
      }),
    );
  }

  clearCurrentUser(): void {
    this.currentUserState.set(null);
  }
}
