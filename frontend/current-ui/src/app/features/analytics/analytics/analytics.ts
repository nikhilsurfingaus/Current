import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { forkJoin } from 'rxjs';

import { AnalyticsService } from '../../../core/services/analytics.service';
import {
  AnalyticsOverview,
  CashFlowMonthPoint,
  CategoryBreakdownItem,
  GoalAnalyticsItem,
  MonthlySummaryResponse,
  NetWorthHistoryPoint,
} from '../../../shared/models';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import { getTransactionCategoryLabel } from '../../../shared/utils/transaction-category.utils';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, PercentPipe],
  templateUrl: './analytics.html',
  styleUrl: './analytics.scss',
})
export class AnalyticsComponent implements OnInit {
  analyticsLoading = signal(false);
  analyticsLoadError = signal('');

  overview = signal<AnalyticsOverview | null>(null);
  cashFlowMonths = signal<CashFlowMonthPoint[]>([]);
  netWorthPoints = signal<NetWorthHistoryPoint[]>([]);
  categoryBreakdown = signal<CategoryBreakdownItem[]>([]);
  goalProgressItems = signal<GoalAnalyticsItem[]>([]);
  monthlySummary = signal<MonthlySummaryResponse | null>(null);

  readonly getTransactionCategoryLabel = getTransactionCategoryLabel;

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

  constructor(private analyticsService: AnalyticsService) {}
}
