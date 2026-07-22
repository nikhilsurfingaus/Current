import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { UserRole } from '../../shared/models';

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const authResponse = authService.getAuthResponse();
  if (authResponse?.role === UserRole.Admin) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
