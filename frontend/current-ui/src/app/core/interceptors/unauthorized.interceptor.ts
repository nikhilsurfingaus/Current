import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { isPublicAuthRequest } from '../../shared/utils/http-error.utils';

function getBearerToken(authorizationHeader: string | null): string | null {
  if (!authorizationHeader?.startsWith('Bearer ')) {
    return null;
  }

  return authorizationHeader.slice('Bearer '.length);
}

export const unauthorizedInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isPublicAuthRequest(request.url)) {
        const requestToken = getBearerToken(request.headers.get('Authorization'));
        const currentToken = authService.getCurrentToken();

        if (requestToken && currentToken && requestToken === currentToken) {
          authService.handleSessionExpired();
        }
      }

      return throwError(() => error);
    }),
  );
};
