import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { DashboardPlaceholderComponent } from './features/dashboard/dashboard-placeholder/dashboard-placeholder';
import { LoginPlaceholderComponent } from './features/auth/login-placeholder/login-placeholder';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginPlaceholderComponent,
  },
  {
    path: 'dashboard',
    component: DashboardPlaceholderComponent,
    canActivate: [authGuard],
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard',
  },
];
