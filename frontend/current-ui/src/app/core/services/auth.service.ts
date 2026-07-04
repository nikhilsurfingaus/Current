import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from '../services/api.service';
import { AuthResponse, LoginRequest, RegisterRequest } from '../../shared/models';
import { AUTH_STORAGE_KEY } from '../auth/auth.constants';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private apiService: ApiService) {}

  login(loginRequest: LoginRequest): Observable<AuthResponse> {
    return this.apiService
      .post<AuthResponse>(API_PATHS.auth.login, loginRequest)
      .pipe(tap((authResponse) => this.persistAuth(authResponse)));
  }

  register(registerRequest: RegisterRequest): Observable<AuthResponse> {
    return this.apiService
      .post<AuthResponse>(API_PATHS.auth.register, registerRequest)
      .pipe(tap((authResponse) => this.persistAuth(authResponse)));
  }

  logout(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
  }

  getToken(): string | null {
    const storedAuth = this.getStoredAuth();
    if (!storedAuth) {
      return null;
    }

    if (this.isTokenExpired(storedAuth.expiresAt)) {
      this.logout();
      return null;
    }

    return storedAuth.token;
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  getAuthResponse(): AuthResponse | null {
    const storedAuth = this.getStoredAuth();
    if (!storedAuth || this.isTokenExpired(storedAuth.expiresAt)) {
      return null;
    }

    return storedAuth;
  }

  private persistAuth(authResponse: AuthResponse): void {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authResponse));
  }

  private getStoredAuth(): AuthResponse | null {
    const authJson = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!authJson) {
      return null;
    }

    try {
      return JSON.parse(authJson) as AuthResponse;
    } catch {
      this.logout();
      return null;
    }
  }

  private isTokenExpired(expiresAt: string): boolean {
    return new Date(expiresAt).getTime() <= Date.now();
  }
}
