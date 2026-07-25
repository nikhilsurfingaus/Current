import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import {
  Account,
  Goal,
  ThemePreference,
  UpdateUserPreferencesRequest,
  UpdateUserProfileRequest,
  User,
} from '../../../shared/models';
import { getAccountTypeLabel } from '../../../shared/utils/account-type.utils';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import {
  focusFirstInvalidControl,
  getControlDescribedBy,
} from '../../../shared/utils/form-accessibility.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import {
  DEFAULT_LOCALE,
  DEFAULT_PREFERRED_CURRENCY,
  DEFAULT_TIMEZONE,
  normalizeUserResponse,
} from '../../../shared/utils/user-preferences.utils';

const THEME_OPTIONS = [
  { value: ThemePreference.Light, label: 'Light' },
  { value: ThemePreference.Dark, label: 'Dark' },
  { value: ThemePreference.System, label: 'System' },
];

const CURRENCY_OPTIONS = ['AUD', 'USD', 'EUR', 'GBP', 'NZD', 'CAD'];
const TIMEZONE_OPTIONS = [
  'Australia/Sydney',
  'Australia/Melbourne',
  'Pacific/Auckland',
  'UTC',
  'America/New_York',
  'America/Los_Angeles',
  'Europe/London',
];
const LOCALE_OPTIONS = [
  { value: 'en-AU', label: 'English (Australia)' },
  { value: 'en-US', label: 'English (United States)' },
  { value: 'en-GB', label: 'English (United Kingdom)' },
  { value: 'en-NZ', label: 'English (New Zealand)' },
];

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SkeletonLoaderComponent],
  templateUrl: './settings.html',
  styleUrl: './settings.scss',
})
export class SettingsComponent implements OnInit {
  pageLoading = signal(true);
  pageLoadError = signal('');
  profileSubmitted = signal(false);
  preferencesSubmitted = signal(false);
  profileSaving = signal(false);
  preferencesSaving = signal(false);
  profileError = signal('');
  preferencesError = signal('');
  userAccounts = signal<Account[]>([]);
  userGoals = signal<Goal[]>([]);

  readonly themeOptions = THEME_OPTIONS;
  readonly currencyOptions = CURRENCY_OPTIONS;
  readonly timezoneOptions = TIMEZONE_OPTIONS;
  readonly localeOptions = LOCALE_OPTIONS;
  readonly getControlDescribedBy = getControlDescribedBy;
  readonly getAccountTypeLabel = getAccountTypeLabel;

  userEmail = computed(() => this.userService.currentUser()?.email ?? '');
  userFacingAccounts = computed(() =>
    filterNonGoalAccounts(this.userAccounts(), this.userGoals()),
  );

  profileForm = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
  });

  preferencesForm = new FormGroup({
    themePreference: new FormControl(ThemePreference.System, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    preferredCurrency: new FormControl('AUD', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^[A-Z]{3}$/)],
    }),
    timezone: new FormControl('Australia/Sydney', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    locale: new FormControl('en-AU', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor(
    private accountService: AccountService,
    private goalService: GoalService,
    private userService: UserService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    const cachedUser = this.userService.currentUser();
    if (cachedUser) {
      this.applyUserToForms(cachedUser);
      this.loadPaymentDetails();
      return;
    }

    this.loadSettings();
  }

  loadPaymentDetails(): void {
    forkJoin({
      accounts: this.accountService.getAllAccounts(),
      goals: this.goalService.getAllGoals(),
    }).subscribe({
      next: ({ accounts, goals }) => {
        this.userAccounts.set(accounts);
        this.userGoals.set(goals);
        this.pageLoading.set(false);
      },
      error: () => {
        this.pageLoading.set(false);
      },
    });
  }

  loadSettings(): void {
    this.pageLoading.set(true);
    this.pageLoadError.set('');

    this.userService.loadCurrentUser().subscribe({
      next: (user) => {
        this.applyUserToForms(user);
        this.loadPaymentDetails();
      },
      error: (error: HttpErrorResponse) => {
        this.pageLoading.set(false);
        this.pageLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load settings.'),
        );
      },
    });
  }

  private applyUserToForms(user: User): void {
    const normalizedUser = normalizeUserResponse(user);

    this.profileForm.patchValue({
      firstName: normalizedUser.firstName,
      lastName: normalizedUser.lastName,
    });
    this.preferencesForm.patchValue({
      themePreference: normalizedUser.themePreference,
      preferredCurrency: normalizedUser.preferredCurrency || DEFAULT_PREFERRED_CURRENCY,
      timezone: normalizedUser.timezone || DEFAULT_TIMEZONE,
      locale: normalizedUser.locale || DEFAULT_LOCALE,
    });
  }

  onSaveProfile(): void {
    this.profileSubmitted.set(true);
    this.profileError.set('');

    if (this.profileForm.invalid) {
      focusFirstInvalidControl(this.profileForm);
      return;
    }

    const request: UpdateUserProfileRequest = this.profileForm.getRawValue();
    this.profileSaving.set(true);

    this.userService.updateProfile(request).subscribe({
      next: () => {
        this.profileSaving.set(false);
        this.profileSubmitted.set(false);
        this.toastService.showSuccess('Profile updated.');
      },
      error: (error: HttpErrorResponse) => {
        this.profileSaving.set(false);
        this.profileError.set(resolveApiErrorMessage(error, 'Unable to update profile.'));
      },
    });
  }

  onSavePreferences(): void {
    this.preferencesSubmitted.set(true);
    this.preferencesError.set('');

    if (this.preferencesForm.invalid) {
      focusFirstInvalidControl(this.preferencesForm);
      return;
    }

    const request: UpdateUserPreferencesRequest = this.preferencesForm.getRawValue();
    this.preferencesSaving.set(true);

    this.userService.updatePreferences(request).subscribe({
      next: () => {
        this.preferencesSaving.set(false);
        this.preferencesSubmitted.set(false);
        this.toastService.showSuccess('Preferences saved.');
      },
      error: (error: HttpErrorResponse) => {
        this.preferencesSaving.set(false);
        this.preferencesError.set(
          resolveApiErrorMessage(error, 'Unable to save preferences.'),
        );
      },
    });
  }
}
