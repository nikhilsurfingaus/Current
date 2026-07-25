import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { guestGuard } from './core/guards/guest.guard';
import { AUTH_TITLE_ROUTE_DATA_KEY } from './core/routing/app-title.constants';
import { AccountsComponent } from './features/accounts/accounts/accounts';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { VerifyEmailComponent } from './features/auth/verify-email/verify-email';
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
import { SettingsComponent } from './features/settings/settings/settings';
import { BranchAdminComponent } from './features/branch/branch-admin/branch-admin';
import { LoansComponent } from './features/loans/loans/loans';
import { LoanDetailComponent } from './features/loans/loan-detail/loan-detail';
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
    path: 'verify-email',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    data: { [AUTH_TITLE_ROUTE_DATA_KEY]: true },
    children: [{ path: '', component: VerifyEmailComponent, title: 'Verify email' }],
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
      {
        path: 'loans',
        component: LoansComponent,
        title: 'Loans',
      },
      {
        path: 'loans/:id',
        component: LoanDetailComponent,
        title: 'Loan',
      },
      {
        path: 'settings',
        component: SettingsComponent,
        title: 'Settings',
      },
      {
        path: 'branch/admin',
        component: BranchAdminComponent,
        canActivate: [adminGuard],
        title: 'Branch admin',
      },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
