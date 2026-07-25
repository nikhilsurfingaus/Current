import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import { ToastService } from './toast.service';
import { UserService } from './user.service';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
  ResendVerificationRequest,
  VerifyEmailRequest,
} from '../../shared/models';
import { AUTH_STORAGE_KEY } from '../auth/auth.constants';
import { SESSION_EXPIRED_MESSAGE } from '../../shared/utils/http-error.utils';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private router = inject(Router);
  private toastService = inject(ToastService);

  private sessionExpiryTimer: ReturnType<typeof setTimeout> | null = null;
  private sessionExpiredInProgress = false;
  private focusListenerRegistered = false;

  constructor(
    private apiService: ApiService,
    private userService: UserService,
  ) {}

  login(loginRequest: LoginRequest): Observable<AuthResponse> {
    return this.apiService
      .post<AuthResponse>(API_PATHS.auth.login, loginRequest)
      .pipe(tap((authResponse) => this.persistAuth(authResponse)));
  }

  register(registerRequest: RegisterRequest): Observable<RegisterResponse> {
    return this.apiService.post<RegisterResponse>(API_PATHS.auth.register, registerRequest);
  }

  verifyEmail(verifyEmailRequest: VerifyEmailRequest): Observable<AuthResponse> {
    return this.apiService
      .post<AuthResponse>(API_PATHS.auth.verifyEmail, verifyEmailRequest)
      .pipe(tap((authResponse) => this.persistAuth(authResponse)));
  }

  resendVerification(
    resendVerificationRequest: ResendVerificationRequest,
  ): Observable<RegisterResponse> {
    return this.apiService.post<RegisterResponse>(
      API_PATHS.auth.resendVerification,
      resendVerificationRequest,
    );
  }

  initializeSession(): void {
    const storedAuth = this.getStoredAuth();

    if (!storedAuth) {
      return;
    }

    if (this.isTokenExpired(storedAuth.expiresAt)) {
      this.handleSessionExpired();
      return;
    }

    this.scheduleSessionExpiry(storedAuth.expiresAt);
    this.registerFocusSessionCheck();
  }

  handleSessionExpired(): void {
    if (this.sessionExpiredInProgress) {
      return;
    }

    this.sessionExpiredInProgress = true;
    this.clearSessionExpiryTimer();
    this.logout();
    this.toastService.showError(SESSION_EXPIRED_MESSAGE);

    void this.router.navigate(['/login']).finally(() => {
      this.sessionExpiredInProgress = false;
    });
  }

  logout(): void {
    this.clearSessionExpiryTimer();
    localStorage.removeItem(AUTH_STORAGE_KEY);
    this.userService.clearCurrentUser();
  }

  getToken(): string | null {
    const storedAuth = this.getStoredAuth();
    if (!storedAuth || this.isTokenExpired(storedAuth.expiresAt)) {
      return null;
    }

    return storedAuth.token;
  }

  getCurrentToken(): string | null {
    return this.getStoredAuth()?.token ?? null;
  }

  isLoggedIn(): boolean {
    const storedAuth = this.getStoredAuth();
    if (!storedAuth) {
      return false;
    }

    return !this.isTokenExpired(storedAuth.expiresAt);
  }

  getAuthResponse(): AuthResponse | null {
    const storedAuth = this.getStoredAuth();
    if (!storedAuth || this.isTokenExpired(storedAuth.expiresAt)) {
      return null;
    }

    return storedAuth;
  }

  private persistAuth(authResponse: AuthResponse): void {
    this.sessionExpiredInProgress = false;
    this.toastService.dismissAll();
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authResponse));
    this.scheduleSessionExpiry(authResponse.expiresAt);
    this.registerFocusSessionCheck();
  }

  private scheduleSessionExpiry(expiresAt: string): void {
    this.clearSessionExpiryTimer();

    const expiryDelayMs = this.parseExpiresAtMs(expiresAt) - Date.now();
    if (expiryDelayMs <= 0) {
      this.handleSessionExpired();
      return;
    }

    this.sessionExpiryTimer = setTimeout(() => this.handleSessionExpired(), expiryDelayMs);
  }

  private clearSessionExpiryTimer(): void {
    if (!this.sessionExpiryTimer) {
      return;
    }

    clearTimeout(this.sessionExpiryTimer);
    this.sessionExpiryTimer = null;
  }

  private registerFocusSessionCheck(): void {
    if (this.focusListenerRegistered || typeof window === 'undefined') {
      return;
    }

    this.focusListenerRegistered = true;

    window.addEventListener('focus', () => {
      const storedAuth = this.getStoredAuth();
      if (storedAuth && this.isTokenExpired(storedAuth.expiresAt)) {
        this.handleSessionExpired();
      }
    });
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
    return this.parseExpiresAtMs(expiresAt) <= Date.now();
  }

  private parseExpiresAtMs(expiresAt: string): number {
    const hasTimezone = expiresAt.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(expiresAt);

    if (hasTimezone) {
      return new Date(expiresAt).getTime();
    }

    return new Date(`${expiresAt}Z`).getTime();
  }
}
