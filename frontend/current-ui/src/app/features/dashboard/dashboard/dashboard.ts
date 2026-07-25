import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ChartConfiguration, ChartData } from 'chart.js';
import { forkJoin } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { GoalService } from '../../../core/services/goal.service';
import { LoanService } from '../../../core/services/loan.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { AppChartComponent } from '../../../shared/components/app-chart/app-chart';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import { GoalIconComponent } from '../../../shared/components/goal-icon/goal-icon';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import {
  Account,
  ApiError,
  Goal,
  GoalStatus,
  Loan,
  LoanStatus,
  NetWorthHistoryPoint,
  Transaction,
} from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { getAccountTypeLabel } from '../../../shared/utils/account-type.utils';
import { buildCurrencyBalanceTotals } from '../../../shared/utils/currency-balance.utils';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';
import { getTransactionFromDisplayName } from '../../../shared/utils/branch-transaction.utils';
import { getLoanStatusLabel } from '../../../shared/utils/loan-status.utils';

const RECENT_TRANSACTION_LIMIT = 5;
const DASHBOARD_GOALS_LIMIT = 3;
const DASHBOARD_LOANS_LIMIT = 2;
const DASHBOARD_ACCOUNTS_PREVIEW_LIMIT = 3;
const CHART_PRIMARY = '#2f80ed';
const CHART_MUTED = '#94a3b8';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CurrencyPipe,
    DecimalPipe,
    RouterLink,
    GoalIconComponent,
    AppChartComponent,
    SkeletonLoaderComponent,
    EmptyStateComponent,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  accounts = signal<Account[]>([]);
  goals = signal<Goal[]>([]);
  loans = signal<Loan[]>([]);
  recentTransactions = signal<Transaction[]>([]);
  netWorthPoints = signal<NetWorthHistoryPoint[]>([]);
  dashboardLoading = signal(false);
  dashboardLoadError = signal('');

  readonly getAccountTypeLabel = getAccountTypeLabel;
  readonly getTransactionStatusLabel = getTransactionStatusLabel;
  readonly goalStatus = GoalStatus;
  readonly loanStatus = LoanStatus;
  readonly getLoanStatusLabel = getLoanStatusLabel;

  userFacingAccounts = computed(() => filterNonGoalAccounts(this.accounts(), this.goals()));

  currencyBalanceTotals = computed(() => buildCurrencyBalanceTotals(this.userFacingAccounts()));

  accountPreview = computed(() =>
    [...this.userFacingAccounts()]
      .sort((left, right) => right.currentBalance - left.currentBalance)
      .slice(0, DASHBOARD_ACCOUNTS_PREVIEW_LIMIT),
  );

  activeGoalsPreview = computed(() =>
    this.goals()
      .filter((goal) => goal.status === GoalStatus.Active)
      .sort((left, right) => right.progressPercent - left.progressPercent)
      .slice(0, DASHBOARD_GOALS_LIMIT),
  );

  activeLoansPreview = computed(() =>
    this.loans()
      .filter((loan) => loan.status === LoanStatus.Active || loan.status === LoanStatus.Overdue || loan.isOverdue)
      .sort((left, right) => Number(right.isOverdue) - Number(left.isOverdue))
      .slice(0, DASHBOARD_LOANS_LIMIT),
  );

  netWorthSparklineData = computed<ChartData>(() => {
    const points = this.netWorthPoints();
    const lastIndex = points.length - 1;

    return {
      labels: points.map((point) => this.formatChartDate(point.date)),
      datasets: [
        {
          label: 'Balance',
          data: points.map((point) => point.balance),
          borderColor: CHART_PRIMARY,
          backgroundColor: 'rgba(47, 128, 237, 0.12)',
          fill: 'start',
          tension: 0.35,
          pointRadius: points.map((_, index) => (index === lastIndex ? 4 : 0)),
          pointHoverRadius: 5,
          pointBackgroundColor: CHART_PRIMARY,
          pointBorderColor: '#ffffff',
          pointBorderWidth: 2,
          borderWidth: 2,
          clip: false,
        },
      ],
    };
  });

  netWorthTrendSummary = computed(() => {
    const points = this.netWorthPoints();
    if (points.length < 2) {
      return null;
    }

    const firstPoint = points[0];
    const lastPoint = points[points.length - 1];
    const balanceChange = lastPoint.balance - firstPoint.balance;
    const changePercent =
      firstPoint.balance !== 0 ? (balanceChange / firstPoint.balance) * 100 : 0;

    return {
      firstPoint,
      lastPoint,
      balanceChange,
      changePercent,
    };
  });

  netWorthSparklineOptions = computed<ChartConfiguration['options']>(() => {
    const currency = this.currencyBalanceTotals()[0]?.currency ?? 'AUD';

    return {
      responsive: true,
      maintainAspectRatio: false,
      animation: false,
      interaction: {
        mode: 'index',
        intersect: false,
      },
      layout: {
        padding: {
          top: 8,
          right: 4,
          left: 0,
          bottom: 0,
        },
      },
      plugins: {
        legend: { display: false },
        tooltip: {
          enabled: true,
          callbacks: {
            title: (items) => {
              const index = items[0]?.dataIndex ?? 0;
              return this.formatChartDate(this.netWorthPoints()[index]?.date ?? '');
            },
            label: (context) => {
              const balance = Number(context.parsed.y ?? 0);
              return this.formatChartCurrency(balance, currency);
            },
          },
        },
        filler: {
          propagate: false,
        },
      },
      scales: {
        x: {
          offset: false,
          bounds: 'data',
          border: { display: false },
          grid: { display: false },
          ticks: {
            color: CHART_MUTED,
            maxTicksLimit: 4,
            maxRotation: 0,
            font: { size: 11 },
          },
        },
        y: {
          display: true,
          beginAtZero: false,
          grace: '8%',
          border: { display: false },
          grid: {
            color: 'rgba(148, 163, 184, 0.2)',
            drawTicks: false,
          },
          ticks: {
            color: CHART_MUTED,
            maxTicksLimit: 3,
            padding: 6,
            font: { size: 11 },
            callback: (value) => this.formatChartCurrency(Number(value), currency),
          },
        },
      },
      elements: {
        line: {
          borderJoinStyle: 'round',
          borderCapStyle: 'round',
        },
      },
    };
  });

  readonly formatChartDate = (dateValue: string): string => {
    const parsedDate = new Date(dateValue);
    if (Number.isNaN(parsedDate.getTime())) {
      return dateValue;
    }

    return parsedDate.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    });
  };

  constructor(
    private accountService: AccountService,
    private analyticsService: AnalyticsService,
    private goalService: GoalService,
    private loanService: LoanService,
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
      loans: this.loanService.getAllLoans(),
      transactions: this.transactionService.getAllTransactions(),
      netWorthHistory: this.analyticsService.getNetWorthHistory(),
    }).subscribe({
      next: (dashboardBundle) => {
        this.accounts.set(dashboardBundle.accounts);
        this.goals.set(dashboardBundle.goals);
        this.loans.set(dashboardBundle.loans);
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

  getTransactionFromName(transaction: Transaction): string {
    return getTransactionFromDisplayName(transaction, (accountId) => this.getAccountName(accountId));
  }

  getAccountCurrency(accountId: string): string {
    return this.accounts().find((account) => account.id === accountId)?.currency ?? 'AUD';
  }

  private formatChartCurrency(value: number, currency: string): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency,
      notation: Math.abs(value) >= 10000 ? 'compact' : 'standard',
      maximumFractionDigits: Math.abs(value) >= 10000 ? 1 : 0,
    }).format(value);
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return fallbackMessage;
  }
}
