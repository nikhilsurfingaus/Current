import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { isPublicAuthRequest } from '../../shared/utils/http-error.utils';

export const unauthorizedInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isPublicAuthRequest(request.url)) {
        authService.handleSessionExpired();
      }

      return throwError(() => error);
    }),
  );
};
