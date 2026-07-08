import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import {
  AnalyticsOverview,
  CashFlowResponse,
  CategoryBreakdownResponse,
  GoalProgressResponse,
  MonthlySummaryResponse,
  NetWorthHistoryResponse,
} from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class AnalyticsService {
  constructor(private apiService: ApiService) {}

  getOverview(): Observable<AnalyticsOverview> {
    return this.apiService.get<AnalyticsOverview>(API_PATHS.analytics.overview);
  }

  getCashFlow(): Observable<CashFlowResponse> {
    return this.apiService.get<CashFlowResponse>(API_PATHS.analytics.cashFlow);
  }

  getNetWorthHistory(): Observable<NetWorthHistoryResponse> {
    return this.apiService.get<NetWorthHistoryResponse>(API_PATHS.analytics.netWorthHistory);
  }

  getCategoryBreakdown(): Observable<CategoryBreakdownResponse> {
    return this.apiService.get<CategoryBreakdownResponse>(API_PATHS.analytics.categories);
  }

  getGoalProgress(): Observable<GoalProgressResponse> {
    return this.apiService.get<GoalProgressResponse>(API_PATHS.analytics.goals);
  }

  getMonthlySummary(): Observable<MonthlySummaryResponse> {
    return this.apiService.get<MonthlySummaryResponse>(API_PATHS.analytics.monthlySummary);
  }
}
