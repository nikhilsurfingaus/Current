import { HttpErrorResponse } from '@angular/common/http';

import { ApiError } from '../models';

export const SESSION_EXPIRED_MESSAGE = 'Your session has expired. Please log in again.';

export function resolveApiErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
  if (error.status === 401) {
    return SESSION_EXPIRED_MESSAGE;
  }

  const apiError = error.error as ApiError | undefined;
  if (apiError?.message) {
    return apiError.message;
  }

  return fallbackMessage;
}

export function isPublicAuthRequest(requestUrl: string): boolean {
  return (
    requestUrl.includes('/auth/login') ||
    requestUrl.includes('/auth/register') ||
    requestUrl.includes('/auth/verify-email') ||
    requestUrl.includes('/auth/resend-verification')
  );
}
