import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { AUTH_TITLE_ROUTE_DATA_KEY } from './core/routing/app-title.constants';
import { AccountsComponent } from './features/accounts/accounts/accounts';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { DashboardComponent } from './features/dashboard/dashboard/dashboard';
import { TransferComponent } from './features/transfer/transfer/transfer';
import { TransactionsComponent } from './features/transactions/transactions/transactions';
import { GoalsComponent } from './features/goals/goals/goals';
import { GoalDetailComponent } from './features/goals/goal-detail/goal-detail';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout';
import { MainLayoutComponent } from './layouts/main-layout/main-layout';

export const routes: Routes = [
  {
    path: 'login',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    data: { [AUTH_TITLE_ROUTE_DATA_KEY]: true },
    children: [{ path: '', component: LoginComponent, title: 'Log on' }],
  },
  {
    path: 'register',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    data: { [AUTH_TITLE_ROUTE_DATA_KEY]: true },
    children: [{ path: '', component: RegisterComponent, title: 'Create account' }],
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
        title: 'Dashboard',
      },
      {
        path: 'accounts',
        component: AccountsComponent,
        title: 'Accounts',
      },
      {
        path: 'transfer',
        component: TransferComponent,
        title: 'Transfer',
      },
      {
        path: 'transactions',
        component: TransactionsComponent,
        title: 'Transactions',
      },
      {
        path: 'goals',
        component: GoalsComponent,
        title: 'Goals',
      },
      {
        path: 'goals/:id',
        component: GoalDetailComponent,
        title: 'Goal',
      },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
