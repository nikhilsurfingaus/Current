import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import {
  ContributeGoalRequest,
  CreateGoalRequest,
  Goal,
  GoalContribution,
  UpdateGoalRequest,
  WithdrawGoalRequest,
} from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class GoalService {
  constructor(private apiService: ApiService) {}

  getAllGoals(): Observable<Goal[]> {
    return this.apiService.get<Goal[]>(API_PATHS.goals.list);
  }

  getGoalById(goalId: string): Observable<Goal> {
    return this.apiService.get<Goal>(API_PATHS.goals.byId(goalId));
  }

  createGoal(createGoalRequest: CreateGoalRequest): Observable<Goal> {
    return this.apiService.post<Goal>(API_PATHS.goals.list, createGoalRequest);
  }

  updateGoal(goalId: string, updateGoalRequest: UpdateGoalRequest): Observable<Goal> {
    return this.apiService.put<Goal>(API_PATHS.goals.byId(goalId), updateGoalRequest);
  }

  cancelGoal(goalId: string): Observable<Goal> {
    return this.apiService.delete<Goal>(API_PATHS.goals.byId(goalId));
  }

  contributeToGoal(goalId: string, contributeGoalRequest: ContributeGoalRequest): Observable<Goal> {
    return this.apiService.post<Goal>(API_PATHS.goals.contribute(goalId), contributeGoalRequest);
  }

  withdrawFromGoal(goalId: string, withdrawGoalRequest: WithdrawGoalRequest): Observable<Goal> {
    return this.apiService.post<Goal>(API_PATHS.goals.withdraw(goalId), withdrawGoalRequest);
  }

  getContributionHistory(goalId: string): Observable<GoalContribution[]> {
    return this.apiService.get<GoalContribution[]>(API_PATHS.goals.history(goalId));
  }
}
