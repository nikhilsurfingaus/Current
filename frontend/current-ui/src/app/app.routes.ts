import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { AccountsComponent } from './features/accounts/accounts/accounts';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout';
import { MainLayoutComponent } from './layouts/main-layout/main-layout';
import { FeaturePlaceholderComponent } from './shared/components/feature-placeholder/feature-placeholder';

export const routes: Routes = [
  {
    path: 'login',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    children: [{ path: '', component: LoginComponent }],
  },
  {
    path: 'register',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    children: [{ path: '', component: RegisterComponent }],
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: FeaturePlaceholderComponent,
        data: {
          pageTitle: 'Dashboard',
          pageSubtitle: 'Overview of your accounts and activity — Part 8',
        },
      },
      {
        path: 'accounts',
        component: AccountsComponent,
      },
      {
        path: 'transfer',
        component: FeaturePlaceholderComponent,
        data: {
          pageTitle: 'Transfer',
          pageSubtitle: 'Move money between accounts — Part 7',
        },
      },
      {
        path: 'transactions',
        component: FeaturePlaceholderComponent,
        data: {
          pageTitle: 'Transactions',
          pageSubtitle: 'View transaction history — Part 7',
        },
      },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
