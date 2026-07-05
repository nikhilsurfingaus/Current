import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { GoalIconComponent } from '../../../shared/components/goal-icon/goal-icon';
import {
  DEFAULT_GOAL_ICON_KEY,
  GOAL_ICON_OPTIONS,
} from '../../../shared/constants/goal-icon-options';
import {
  Account,
  CreateGoalRequest,
  Goal,
  GoalStatus,
} from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import {
  GOAL_STATUS_FILTER_OPTIONS,
  getGoalStatusLabel,
} from '../../../shared/utils/goal-status.utils';

@Component({
  selector: 'app-goals',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe, PercentPipe, GoalIconComponent],
  templateUrl: './goals.html',
  styleUrl: './goals.scss',
})
export class GoalsComponent implements OnInit {
  goals = signal<Goal[]>([]);
  fundingAccounts = signal<Account[]>([]);
  goalsLoading = signal(false);
  goalsLoadError = signal('');
  createPanelOpen = signal(false);
  createFormSubmitted = signal(false);
  createRequestInFlight = signal(false);
  createErrorMessage = signal('');
  statusFilter = signal<GoalStatus | null>(null);

  readonly goalIconOptions = GOAL_ICON_OPTIONS;
  readonly statusFilterOptions = GOAL_STATUS_FILTER_OPTIONS;
  readonly goalStatus = GoalStatus;
  readonly getGoalStatusLabel = getGoalStatusLabel;

  filteredGoals = computed(() => {
    const selectedStatus = this.statusFilter();

    if (selectedStatus === null) {
      return this.goals();
    }

    return this.goals().filter((goal) => goal.status === selectedStatus);
  });

  totalSavedAmount = computed(() =>
    this.goals()
      .filter((goal) => goal.status !== GoalStatus.Cancelled)
      .reduce((total, goal) => total + goal.currentAmount, 0),
  );

  activeGoalsCount = computed(
    () => this.goals().filter((goal) => goal.status === GoalStatus.Active).length,
  );

  completedGoalsCount = computed(
    () => this.goals().filter((goal) => goal.status === GoalStatus.Completed).length,
  );

  summaryCurrency = computed(() => this.goals().find((goal) => goal.currency)?.currency ?? 'AUD');

  createGoalForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
    targetAmount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    currency: new FormControl('AUD', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(3)],
    }),
    sourceAccountId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    targetDate: new FormControl(''),
    iconKey: new FormControl(DEFAULT_GOAL_ICON_KEY, {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor(
    private goalService: GoalService,
    private accountService: AccountService,
  ) {}

  ngOnInit(): void {
    this.loadGoalsPageData();
  }

  loadGoalsPageData(): void {
    this.goalsLoading.set(true);
    this.goalsLoadError.set('');

    this.goalService.getAllGoals().subscribe({
      next: (goals) => {
        this.goals.set(goals);
        this.loadFundingAccounts();
      },
      error: (error: HttpErrorResponse) => {
        this.goalsLoading.set(false);
        this.goalsLoadError.set(this.resolveErrorMessage(error, 'Unable to load goals.'));
      },
    });
  }

  setStatusFilter(status: GoalStatus | null): void {
    this.statusFilter.set(status);
  }

  isStatusFilterActive(status: GoalStatus | null): boolean {
    return this.statusFilter() === status;
  }

  openCreatePanel(): void {
    this.createPanelOpen.set(true);
    this.createFormSubmitted.set(false);
    this.createErrorMessage.set('');
    this.createGoalForm.reset({
      name: '',
      description: '',
      targetAmount: 0,
      currency: this.fundingAccounts()[0]?.currency ?? 'AUD',
      sourceAccountId: this.fundingAccounts()[0]?.id ?? '',
      targetDate: '',
      iconKey: DEFAULT_GOAL_ICON_KEY,
    });
  }

  closeCreatePanel(): void {
    this.createPanelOpen.set(false);
    this.createFormSubmitted.set(false);
    this.createErrorMessage.set('');
  }

  selectCreateIcon(iconKey: string): void {
    this.createGoalForm.controls.iconKey.setValue(iconKey);
  }

  isCreateIconSelected(iconKey: string): boolean {
    return this.createGoalForm.controls.iconKey.value === iconKey;
  }

  onSourceAccountChange(): void {
    const selectedAccount = this.fundingAccounts().find(
      (account) => account.id === this.createGoalForm.controls.sourceAccountId.value,
    );

    if (selectedAccount) {
      this.createGoalForm.controls.currency.setValue(selectedAccount.currency);
    }
  }

  onCreateGoal(): void {
    this.createFormSubmitted.set(true);
    this.createErrorMessage.set('');

    if (this.createGoalForm.invalid) {
      return;
    }

    const formValues = this.createGoalForm.getRawValue();
    const createGoalRequest: CreateGoalRequest = {
      name: formValues.name.trim(),
      description: formValues.description.trim() || null,
      targetAmount: formValues.targetAmount,
      currency: formValues.currency.trim().toUpperCase(),
      sourceAccountId: formValues.sourceAccountId,
      targetDate: formValues.targetDate || null,
      iconKey: formValues.iconKey,
    };

    this.createRequestInFlight.set(true);

    this.goalService.createGoal(createGoalRequest).subscribe({
      next: (createdGoal) => {
        this.createRequestInFlight.set(false);
        this.goals.set(
          [...this.goals(), createdGoal].sort((left, right) => left.name.localeCompare(right.name)),
        );
        this.loadFundingAccounts();
        this.closeCreatePanel();
      },
      error: (error: HttpErrorResponse) => {
        this.createRequestInFlight.set(false);
        this.createErrorMessage.set(this.resolveErrorMessage(error, 'Unable to create goal.'));
      },
    });
  }

  private loadFundingAccounts(): void {
    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        const nonGoalAccounts = filterNonGoalAccounts(accounts, this.goals());
        this.fundingAccounts.set(nonGoalAccounts);
        this.goalsLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.goalsLoading.set(false);
        this.goalsLoadError.set(this.resolveErrorMessage(error, 'Unable to load accounts.'));
      },
    });
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    return resolveApiErrorMessage(error, fallbackMessage);
  }
}
