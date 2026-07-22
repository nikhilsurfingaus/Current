import { CurrencyPipe, PercentPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, OnInit, computed, signal } from '@angular/core';
import { ChartConfiguration, ChartData } from 'chart.js';
import { forkJoin } from 'rxjs';

import { AnalyticsService } from '../../../core/services/analytics.service';
import { UserService } from '../../../core/services/user.service';
import { AppChartComponent } from '../../../shared/components/app-chart/app-chart';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import {
  AnalyticsOverview,
  CashFlowMonthPoint,
  CategoryBreakdownItem,
  GoalAnalyticsItem,
  MonthlySummaryResponse,
  NetWorthHistoryPoint,
} from '../../../shared/models';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import { getPreferredCurrency } from '../../../shared/utils/user-preferences.utils';
import { getTransactionCategoryLabel } from '../../../shared/utils/transaction-category.utils';

const CHART_PRIMARY = '#2f80ed';
const CHART_SUCCESS = '#16a34a';
const CHART_ERROR = '#dc2626';
const CHART_MUTED = '#94a3b8';
const COMPACT_BREAKPOINT_PX = 700;
const CATEGORY_COLORS = [
  '#2f80ed',
  '#16a34a',
  '#f59e0b',
  '#dc2626',
  '#8b5cf6',
  '#06b6d4',
  '#ec4899',
  '#64748b',
  '#84cc16',
  '#f97316',
];

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CurrencyPipe, PercentPipe, AppChartComponent, SkeletonLoaderComponent],
  templateUrl: './analytics.html',
  styleUrl: './analytics.scss',
})
export class AnalyticsComponent implements OnInit {
  analyticsLoading = signal(false);
  analyticsLoadError = signal('');
  isCompactViewport = signal(
    typeof window !== 'undefined' && window.innerWidth <= COMPACT_BREAKPOINT_PX,
  );

  overview = signal<AnalyticsOverview | null>(null);
  cashFlowMonths = signal<CashFlowMonthPoint[]>([]);
  netWorthPoints = signal<NetWorthHistoryPoint[]>([]);
  categoryBreakdown = signal<CategoryBreakdownItem[]>([]);
  goalProgressItems = signal<GoalAnalyticsItem[]>([]);
  monthlySummary = signal<MonthlySummaryResponse | null>(null);

  readonly getTransactionCategoryLabel = getTransactionCategoryLabel;

  displayCurrency = computed(() => getPreferredCurrency(this.userService.currentUser()));

  chartHeight = computed(() => (this.isCompactViewport() ? '220px' : '280px'));
  categoryChartHeight = computed(() => (this.isCompactViewport() ? '200px' : '260px'));

  cashFlowChartData = computed<ChartData>(() => {
    const months = this.cashFlowMonths();
    const compact = this.isCompactViewport();

    return {
      labels: months.map((month) => this.formatMonthLabel(month.month, compact)),
      datasets: [
        {
          label: 'Income',
          data: months.map((month) => month.income),
          backgroundColor: CHART_SUCCESS,
          borderRadius: 6,
          maxBarThickness: compact ? 18 : 28,
        },
        {
          label: 'Expenses',
          data: months.map((month) => month.expenses),
          backgroundColor: CHART_ERROR,
          borderRadius: 6,
          maxBarThickness: compact ? 18 : 28,
        },
      ],
    };
  });

  cashFlowChartOptions = computed<ChartConfiguration['options']>(() => {
    const compact = this.isCompactViewport();

    if (compact) {
      return {
        animation: false,
        indexAxis: 'y',
        plugins: {
          legend: {
            position: 'bottom',
            labels: { boxWidth: 10, usePointStyle: true, font: { size: 11 } },
          },
        },
        scales: {
          x: {
            beginAtZero: true,
            ticks: {
              color: CHART_MUTED,
              maxTicksLimit: 4,
              callback: (value) => `$${Number(value).toLocaleString()}`,
            },
          },
          y: {
            grid: { display: false },
            ticks: { color: CHART_MUTED, font: { size: 11 } },
          },
        },
      };
    }

    return {
      animation: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: { boxWidth: 12, usePointStyle: true },
        },
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: { color: CHART_MUTED },
        },
        y: {
          beginAtZero: true,
          ticks: {
            color: CHART_MUTED,
            callback: (value) => `$${Number(value).toLocaleString()}`,
          },
        },
      },
    };
  });

  categoryChartData = computed<ChartData>(() => {
    const categories = this.categoryBreakdown();
    return {
      labels: categories.map((item) => getTransactionCategoryLabel(item.category)),
      datasets: [
        {
          data: categories.map((item) => item.amount),
          backgroundColor: categories.map((_, index) => CATEGORY_COLORS[index % CATEGORY_COLORS.length]),
          borderWidth: 0,
          hoverOffset: 4,
        },
      ],
    };
  });

  categoryChartOptions = computed<ChartConfiguration['options']>(() => ({
    animation: false,
    plugins: {
      legend: {
        display: !this.isCompactViewport(),
        position: 'bottom',
        labels: { boxWidth: 12, usePointStyle: true },
      },
    },
  }));

  netWorthChartData = computed<ChartData>(() => {
    const points = this.netWorthPoints();
    return {
      labels: points.map((point) => this.formatDayLabel(point.date)),
      datasets: [
        {
          label: 'Net worth',
          data: points.map((point) => point.balance),
          borderColor: CHART_PRIMARY,
          backgroundColor: 'rgba(47, 128, 237, 0.12)',
          fill: true,
          tension: 0.35,
          pointRadius: 0,
          pointHoverRadius: 4,
          borderWidth: 2,
        },
      ],
    };
  });

  netWorthChartOptions = computed<ChartConfiguration['options']>(() => {
    const compact = this.isCompactViewport();

    return {
      animation: false,
      plugins: {
        legend: { display: false },
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: {
            color: CHART_MUTED,
            maxTicksLimit: compact ? 4 : 6,
            font: { size: compact ? 10 : 12 },
          },
        },
        y: {
          ticks: {
            color: CHART_MUTED,
            maxTicksLimit: compact ? 4 : undefined,
            callback: (value) => `$${Number(value).toLocaleString()}`,
          },
        },
      },
    };
  });

  recentCashFlowMonths = computed(() => {
    const months = this.cashFlowMonths();
    return months.slice(-3).reverse();
  });

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.analyticsLoading.set(true);
    this.analyticsLoadError.set('');

    forkJoin({
      overview: this.analyticsService.getOverview(),
      cashFlow: this.analyticsService.getCashFlow(),
      netWorthHistory: this.analyticsService.getNetWorthHistory(),
      categories: this.analyticsService.getCategoryBreakdown(),
      goals: this.analyticsService.getGoalProgress(),
      monthlySummary: this.analyticsService.getMonthlySummary(),
    }).subscribe({
      next: (analyticsBundle) => {
        this.overview.set(analyticsBundle.overview);
        this.cashFlowMonths.set(analyticsBundle.cashFlow.months);
        this.netWorthPoints.set(analyticsBundle.netWorthHistory.points);
        this.categoryBreakdown.set(analyticsBundle.categories.categories);
        this.goalProgressItems.set(analyticsBundle.goals.goals);
        this.monthlySummary.set(analyticsBundle.monthlySummary);
        this.analyticsLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.analyticsLoading.set(false);
        this.analyticsLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load analytics right now.'),
        );
      },
    });
  }

  constructor(
    private analyticsService: AnalyticsService,
    private userService: UserService,
  ) {}

  @HostListener('window:resize')
  onWindowResize(): void {
    this.isCompactViewport.set(window.innerWidth <= COMPACT_BREAKPOINT_PX);
  }

  formatMonthLabelPublic(monthValue: string): string {
    return this.formatMonthLabel(monthValue, this.isCompactViewport());
  }

  private formatMonthLabel(monthValue: string, compact = false): string {
    const parsedDate = new Date(`${monthValue}-01T00:00:00`);
    if (Number.isNaN(parsedDate.getTime())) {
      return monthValue;
    }

    return parsedDate.toLocaleDateString(undefined, {
      month: 'short',
      year: compact ? undefined : '2-digit',
    });
  }

  private formatDayLabel(dateValue: string): string {
    const parsedDate = new Date(dateValue);
    if (Number.isNaN(parsedDate.getTime())) {
      return dateValue;
    }

    return parsedDate.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  }
}
