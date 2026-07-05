import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { GoalIconComponent } from '../../../shared/components/goal-icon/goal-icon';
import { Account, ApiError, Goal, GoalStatus, Transaction } from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { getAccountTypeLabel } from '../../../shared/utils/account-type.utils';
import { buildCurrencyBalanceTotals } from '../../../shared/utils/currency-balance.utils';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';

const RECENT_TRANSACTION_LIMIT = 5;
const DASHBOARD_GOALS_LIMIT = 3;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, GoalIconComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  accounts = signal<Account[]>([]);
  goals = signal<Goal[]>([]);
  recentTransactions = signal<Transaction[]>([]);
  dashboardLoading = signal(false);
  dashboardLoadError = signal('');

  readonly getAccountTypeLabel = getAccountTypeLabel;
  readonly getTransactionStatusLabel = getTransactionStatusLabel;
  readonly goalStatus = GoalStatus;

  userFacingAccounts = computed(() => filterNonGoalAccounts(this.accounts(), this.goals()));

  currencyBalanceTotals = computed(() => buildCurrencyBalanceTotals(this.userFacingAccounts()));

  activeGoalsPreview = computed(() =>
    this.goals()
      .filter((goal) => goal.status === GoalStatus.Active)
      .sort((left, right) => right.progressPercent - left.progressPercent)
      .slice(0, DASHBOARD_GOALS_LIMIT),
  );

  constructor(
    private accountService: AccountService,
    private goalService: GoalService,
    private transactionService: TransactionService,
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.dashboardLoading.set(true);
    this.dashboardLoadError.set('');

    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        this.accounts.set(accounts);
        this.loadGoals();
      },
      error: (error: HttpErrorResponse) => {
        this.dashboardLoading.set(false);
        this.dashboardLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load dashboard.'),
        );
      },
    });
  }

  loadGoals(): void {
    this.goalService.getAllGoals().subscribe({
      next: (goals) => {
        this.goals.set(goals);
        this.loadRecentTransactions();
      },
      error: (error: HttpErrorResponse) => {
        this.dashboardLoading.set(false);
        this.dashboardLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load goals.'),
        );
      },
    });
  }

  loadRecentTransactions(): void {
    this.transactionService.getAllTransactions().subscribe({
      next: (transactions) => {
        this.recentTransactions.set(transactions.slice(0, RECENT_TRANSACTION_LIMIT));
        this.dashboardLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.dashboardLoading.set(false);
        this.dashboardLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load recent activity.'),
        );
      },
    });
  }

  getAccountName(accountId: string): string {
    return this.accounts().find((account) => account.id === accountId)?.name ?? 'Unknown account';
  }

  getAccountCurrency(accountId: string): string {
    return this.accounts().find((account) => account.id === accountId)?.currency ?? 'AUD';
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return fallbackMessage;
  }
}
