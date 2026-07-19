import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { AUTH_TITLE_ROUTE_DATA_KEY } from './core/routing/app-title.constants';
import { AccountsComponent } from './features/accounts/accounts/accounts';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ContactsComponent } from './features/contacts/contacts/contacts';
import { DashboardComponent } from './features/dashboard/dashboard/dashboard';
import { AnalyticsComponent } from './features/analytics/analytics/analytics';
import { TransferComponent } from './features/transfer/transfer/transfer';
import { TransactionsComponent } from './features/transactions/transactions/transactions';
import { GoalsComponent } from './features/goals/goals/goals';
import { GoalDetailComponent } from './features/goals/goal-detail/goal-detail';
import { PaymentHistoryComponent } from './features/payments/payment-history/payment-history';
import { PaySomeoneComponent } from './features/payments/pay-someone/pay-someone';
import { PaymentReceiptComponent } from './features/payments/payment-receipt/payment-receipt';
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
        path: 'payments/send',
        component: PaySomeoneComponent,
        title: 'Pay someone',
      },
      {
        path: 'payments/:transactionId',
        component: PaymentReceiptComponent,
        title: 'Payment receipt',
      },
      {
        path: 'payments',
        component: PaymentHistoryComponent,
        title: 'Payments',
      },
      {
        path: 'contacts',
        component: ContactsComponent,
        title: 'Contacts',
      },
      {
        path: 'transactions',
        component: TransactionsComponent,
        title: 'Transactions',
      },
      {
        path: 'analytics',
        component: AnalyticsComponent,
        title: 'Analytics',
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
