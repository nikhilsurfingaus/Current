import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { ToastService } from '../../../core/services/toast.service';
import { GoalIconComponent } from '../../../shared/components/goal-icon/goal-icon';
import { GOAL_ICON_OPTIONS } from '../../../shared/constants/goal-icon-options';
import {
  Account,
  ApiError,
  ContributeGoalRequest,
  Goal,
  GoalContribution,
  GoalStatus,
  UpdateGoalRequest,
  WithdrawGoalRequest,
} from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { getContributionTypeLabel } from '../../../shared/utils/contribution-type.utils';
import { getGoalStatusLabel } from '../../../shared/utils/goal-status.utils';

@Component({
  selector: 'app-goal-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    PercentPipe,
    GoalIconComponent,
  ],
  templateUrl: './goal-detail.html',
  styleUrl: './goal-detail.scss',
})
export class GoalDetailComponent implements OnInit {
  goal = signal<Goal | null>(null);
  contributionHistory = signal<GoalContribution[]>([]);
  destinationAccounts = signal<Account[]>([]);
  sourceAccountName = signal('');
  pageLoading = signal(false);
  pageLoadError = signal('');
  historyLoading = signal(false);

  contributePanelOpen = signal(false);
  withdrawPanelOpen = signal(false);
  editPanelOpen = signal(false);
  contributeFormSubmitted = signal(false);
  withdrawFormSubmitted = signal(false);
  editFormSubmitted = signal(false);
  contributeRequestInFlight = signal(false);
  withdrawRequestInFlight = signal(false);
  editRequestInFlight = signal(false);
  cancelRequestInFlight = signal(false);
  actionErrorMessage = signal('');

  readonly goalStatus = GoalStatus;
  readonly goalIconOptions = GOAL_ICON_OPTIONS;
  readonly getGoalStatusLabel = getGoalStatusLabel;
  readonly getContributionTypeLabel = getContributionTypeLabel;

  contributeForm = new FormGroup({
    amount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    note: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
  });

  withdrawForm = new FormGroup({
    amount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    destinationAccountId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    note: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
  });

  editGoalForm = new FormGroup({
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
    targetDate: new FormControl(''),
    iconKey: new FormControl('default', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private goalService: GoalService,
    private accountService: AccountService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const goalId = params.get('id');

      if (!goalId) {
        void this.router.navigate(['/goals']);
        return;
      }

      this.loadGoalDetail(goalId);
    });
  }

  get goalId(): string {
    return this.route.snapshot.paramMap.get('id') ?? '';
  }

  canContribute(): boolean {
    const currentGoal = this.goal();
    return currentGoal?.status === GoalStatus.Active;
  }

  canWithdraw(): boolean {
    const currentGoal = this.goal();
    return currentGoal?.status === GoalStatus.Active || currentGoal?.status === GoalStatus.Completed;
  }

  canEdit(): boolean {
    const currentGoal = this.goal();
    return currentGoal?.status !== GoalStatus.Cancelled;
  }

  canCancel(): boolean {
    const currentGoal = this.goal();
    return currentGoal?.status !== GoalStatus.Cancelled;
  }

  openContributePanel(): void {
    this.closeActionPanels();
    this.contributePanelOpen.set(true);
    this.contributeFormSubmitted.set(false);
    this.actionErrorMessage.set('');
    this.contributeForm.reset({ amount: 0, note: '' });
  }

  openWithdrawPanel(): void {
    this.closeActionPanels();
    this.withdrawPanelOpen.set(true);
    this.withdrawFormSubmitted.set(false);
    this.actionErrorMessage.set('');
    this.withdrawForm.reset({
      amount: 0,
      destinationAccountId: this.destinationAccounts()[0]?.id ?? '',
      note: '',
    });
  }

  openEditPanel(): void {
    const currentGoal = this.goal();

    if (!currentGoal) {
      return;
    }

    this.closeActionPanels();
    this.editPanelOpen.set(true);
    this.editFormSubmitted.set(false);
    this.actionErrorMessage.set('');
    this.editGoalForm.reset({
      name: currentGoal.name,
      description: currentGoal.description ?? '',
      targetAmount: currentGoal.targetAmount,
      targetDate: currentGoal.targetDate ?? '',
      iconKey: currentGoal.iconKey,
    });
  }

  closeActionPanels(): void {
    this.contributePanelOpen.set(false);
    this.withdrawPanelOpen.set(false);
    this.editPanelOpen.set(false);
    this.contributeFormSubmitted.set(false);
    this.withdrawFormSubmitted.set(false);
    this.editFormSubmitted.set(false);
    this.actionErrorMessage.set('');
  }

  selectEditIcon(iconKey: string): void {
    this.editGoalForm.controls.iconKey.setValue(iconKey);
  }

  isEditIconSelected(iconKey: string): boolean {
    return this.editGoalForm.controls.iconKey.value === iconKey;
  }

  onContribute(): void {
    this.contributeFormSubmitted.set(true);
    this.actionErrorMessage.set('');

    if (this.contributeForm.invalid) {
      return;
    }

    const formValues = this.contributeForm.getRawValue();
    const contributeGoalRequest: ContributeGoalRequest = {
      amount: formValues.amount,
      note: formValues.note.trim() || null,
    };

    this.contributeRequestInFlight.set(true);

    this.goalService.contributeToGoal(this.goalId, contributeGoalRequest).subscribe({
      next: (updatedGoal) => {
        this.contributeRequestInFlight.set(false);
        this.goal.set(updatedGoal);
        this.closeActionPanels();
        this.loadContributionHistory(this.goalId);
        this.toastService.showSuccess('Contribution added successfully.');
      },
      error: (error: HttpErrorResponse) => {
        this.contributeRequestInFlight.set(false);
        this.actionErrorMessage.set(this.resolveErrorMessage(error, 'Unable to contribute.'));
      },
    });
  }

  onWithdraw(): void {
    this.withdrawFormSubmitted.set(true);
    this.actionErrorMessage.set('');

    if (this.withdrawForm.invalid) {
      return;
    }

    const formValues = this.withdrawForm.getRawValue();
    const withdrawGoalRequest: WithdrawGoalRequest = {
      amount: formValues.amount,
      destinationAccountId: formValues.destinationAccountId,
      note: formValues.note.trim() || null,
    };

    this.withdrawRequestInFlight.set(true);

    this.goalService.withdrawFromGoal(this.goalId, withdrawGoalRequest).subscribe({
      next: (updatedGoal) => {
        this.withdrawRequestInFlight.set(false);
        this.goal.set(updatedGoal);
        this.closeActionPanels();
        this.loadContributionHistory(this.goalId);
        this.toastService.showSuccess('Withdrawal completed successfully.');
      },
      error: (error: HttpErrorResponse) => {
        this.withdrawRequestInFlight.set(false);
        this.actionErrorMessage.set(this.resolveErrorMessage(error, 'Unable to withdraw.'));
      },
    });
  }

  onEditGoal(): void {
    const currentGoal = this.goal();

    if (!currentGoal) {
      return;
    }

    this.editFormSubmitted.set(true);
    this.actionErrorMessage.set('');

    if (this.editGoalForm.invalid) {
      return;
    }

    const formValues = this.editGoalForm.getRawValue();
    const updateGoalRequest: UpdateGoalRequest = {
      name: formValues.name.trim(),
      description: formValues.description.trim() || null,
      targetAmount: formValues.targetAmount,
      targetDate: formValues.targetDate || null,
      status: currentGoal.status,
      iconKey: formValues.iconKey,
    };

    this.editRequestInFlight.set(true);

    this.goalService.updateGoal(this.goalId, updateGoalRequest).subscribe({
      next: (updatedGoal) => {
        this.editRequestInFlight.set(false);
        this.goal.set(updatedGoal);
        this.closeActionPanels();
        this.toastService.showSuccess('Goal updated successfully.');
      },
      error: (error: HttpErrorResponse) => {
        this.editRequestInFlight.set(false);
        this.actionErrorMessage.set(this.resolveErrorMessage(error, 'Unable to update goal.'));
      },
    });
  }

  onCancelGoal(): void {
    if (!this.canCancel() || this.cancelRequestInFlight()) {
      return;
    }

    this.cancelRequestInFlight.set(true);
    this.actionErrorMessage.set('');

    this.goalService.cancelGoal(this.goalId).subscribe({
      next: (cancelledGoal) => {
        this.cancelRequestInFlight.set(false);
        this.goal.set(cancelledGoal);
        this.toastService.showSuccess('Goal cancelled.');
      },
      error: (error: HttpErrorResponse) => {
        this.cancelRequestInFlight.set(false);
        this.actionErrorMessage.set(this.resolveErrorMessage(error, 'Unable to cancel goal.'));
      },
    });
  }

  private loadGoalDetail(goalId: string): void {
    this.pageLoading.set(true);
    this.pageLoadError.set('');

    this.goalService.getGoalById(goalId).subscribe({
      next: (goal) => {
        this.goal.set(goal);
        this.loadContributionHistory(goalId);
        this.loadAccounts(goal);
      },
      error: (error: HttpErrorResponse) => {
        this.pageLoading.set(false);
        this.pageLoadError.set(this.resolveErrorMessage(error, 'Unable to load goal.'));
      },
    });
  }

  private loadContributionHistory(goalId: string): void {
    this.historyLoading.set(true);

    this.goalService.getContributionHistory(goalId).subscribe({
      next: (history) => {
        this.contributionHistory.set(history);
        this.historyLoading.set(false);
      },
      error: () => {
        this.historyLoading.set(false);
      },
    });
  }

  private loadAccounts(goal: Goal): void {
    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        this.goalService.getAllGoals().subscribe({
          next: (goals) => {
            const nonGoalAccounts = filterNonGoalAccounts(accounts, goals);
            this.destinationAccounts.set(
              nonGoalAccounts.filter((account) => account.currency === goal.currency),
            );
            this.sourceAccountName.set(
              accounts.find((account) => account.id === goal.sourceAccountId)?.name ?? 'Unknown account',
            );
            this.pageLoading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.pageLoading.set(false);
            this.pageLoadError.set(this.resolveErrorMessage(error, 'Unable to load accounts.'));
          },
        });
      },
      error: (error: HttpErrorResponse) => {
        this.pageLoading.set(false);
        this.pageLoadError.set(this.resolveErrorMessage(error, 'Unable to load accounts.'));
      },
    });
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return fallbackMessage;
  }
}
