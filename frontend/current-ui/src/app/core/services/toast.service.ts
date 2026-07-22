import { Injectable, signal } from '@angular/core';

export const DEFAULT_TOAST_DURATION_MS = 10_000;

export type ToastType = 'success' | 'error';

export interface ToastAction {
  label: string;
  route: string;
}

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  durationMs: number;
  action?: ToastAction;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly toastsState = signal<Toast[]>([]);
  private readonly dismissTimers = new Map<string, ReturnType<typeof setTimeout>>();

  readonly toasts = this.toastsState.asReadonly();

  showSuccess(message: string, action?: ToastAction, durationMs = DEFAULT_TOAST_DURATION_MS): void {
    this.addToast({ message, type: 'success', action, durationMs });
  }

  showError(message: string, action?: ToastAction, durationMs = DEFAULT_TOAST_DURATION_MS): void {
    this.addToast({ message, type: 'error', action, durationMs });
  }

  dismiss(toastId: string): void {
    this.clearTimer(toastId);
    this.toastsState.update((toasts) => toasts.filter((toast) => toast.id !== toastId));
  }

  dismissAll(): void {
    for (const toastId of [...this.dismissTimers.keys()]) {
      this.clearTimer(toastId);
    }

    this.toastsState.set([]);
  }

  private addToast(toast: Omit<Toast, 'id'>): void {
    const toastId = crypto.randomUUID();
    const nextToast: Toast = { ...toast, id: toastId };

    this.toastsState.update((toasts) => [...toasts, nextToast]);

    const timer = setTimeout(() => this.dismiss(toastId), toast.durationMs);
    this.dismissTimers.set(toastId, timer);
  }

  private clearTimer(toastId: string): void {
    const timer = this.dismissTimers.get(toastId);
    if (!timer) {
      return;
    }

    clearTimeout(timer);
    this.dismissTimers.delete(toastId);
  }
}
