import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ChartConfiguration, ChartData } from 'chart.js';
import { forkJoin } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { GoalService } from '../../../core/services/goal.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { AppChartComponent } from '../../../shared/components/app-chart/app-chart';
import { GoalIconComponent } from '../../../shared/components/goal-icon/goal-icon';
import {
  Account,
  ApiError,
  Goal,
  GoalStatus,
  NetWorthHistoryPoint,
  Transaction,
} from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { getAccountTypeLabel } from '../../../shared/utils/account-type.utils';
import { buildCurrencyBalanceTotals } from '../../../shared/utils/currency-balance.utils';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';

const RECENT_TRANSACTION_LIMIT = 5;
const DASHBOARD_GOALS_LIMIT = 3;
const CHART_PRIMARY = '#2f80ed';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, GoalIconComponent, AppChartComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  accounts = signal<Account[]>([]);
  goals = signal<Goal[]>([]);
  recentTransactions = signal<Transaction[]>([]);
  netWorthPoints = signal<NetWorthHistoryPoint[]>([]);
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

  netWorthSparklineData = computed<ChartData>(() => {
    const points = this.netWorthPoints();
    return {
      labels: points.map((point) => point.date),
      datasets: [
        {
          data: points.map((point) => point.balance),
          borderColor: CHART_PRIMARY,
          backgroundColor: 'rgba(47, 128, 237, 0.15)',
          fill: true,
          tension: 0.4,
          pointRadius: 0,
          borderWidth: 2,
        },
      ],
    };
  });

  netWorthSparklineOptions: ChartConfiguration['options'] = {
    plugins: {
      legend: { display: false },
      tooltip: { enabled: false },
    },
    scales: {
      x: { display: false },
      y: { display: false },
    },
    elements: {
      line: { borderJoinStyle: 'round' },
    },
  };

  constructor(
    private accountService: AccountService,
    private analyticsService: AnalyticsService,
    private goalService: GoalService,
    private transactionService: TransactionService,
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.dashboardLoading.set(true);
    this.dashboardLoadError.set('');

    forkJoin({
      accounts: this.accountService.getAllAccounts(),
      goals: this.goalService.getAllGoals(),
      transactions: this.transactionService.getAllTransactions(),
      netWorthHistory: this.analyticsService.getNetWorthHistory(),
    }).subscribe({
      next: (dashboardBundle) => {
        this.accounts.set(dashboardBundle.accounts);
        this.goals.set(dashboardBundle.goals);
        this.recentTransactions.set(dashboardBundle.transactions.slice(0, RECENT_TRANSACTION_LIMIT));
        this.netWorthPoints.set(dashboardBundle.netWorthHistory.points);
        this.dashboardLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.dashboardLoading.set(false);
        this.dashboardLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load dashboard.'),
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
