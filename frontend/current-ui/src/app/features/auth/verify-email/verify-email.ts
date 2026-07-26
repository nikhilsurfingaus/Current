import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';
import { AuthMarkComponent } from '../../../shared/components/auth-mark/auth-mark';
import { ApiError } from '../../../shared/models';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';

const RESEND_COOLDOWN_SECONDS = 10 * 60;
const VERIFICATION_CODE_PATTERN = /^\d{6}$/;

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthMarkComponent],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.scss',
})
export class VerifyEmailComponent implements OnInit, OnDestroy {
  verifyEmailFormSubmitted = signal(false);
  verifyEmailErrorMessage = signal('');
  verifyEmailSuccessMessage = signal('');
  verifyRequestInFlight = signal(false);
  resendRequestInFlight = signal(false);
  resendCooldownSeconds = signal(0);
  resendCooldownLabel = signal('');
  verifyEmailAddress = signal('');

  verifyEmailForm = new FormGroup({
    code: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(VERIFICATION_CODE_PATTERN)],
    }),
  });

  private resendCooldownTimer: ReturnType<typeof setInterval> | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const emailQueryParam = this.route.snapshot.queryParamMap.get('email')?.trim() ?? '';

    if (!emailQueryParam) {
      void this.router.navigate(['/register']);
      return;
    }

    this.verifyEmailAddress.set(emailQueryParam);
    this.startResendCooldown(RESEND_COOLDOWN_SECONDS);
  }

  ngOnDestroy(): void {
    this.clearResendCooldownTimer();
  }

  onSubmit(): void {
    this.verifyEmailFormSubmitted.set(true);
    this.verifyEmailErrorMessage.set('');
    this.verifyEmailSuccessMessage.set('');

    if (this.verifyEmailForm.invalid) {
      focusFirstInvalidControl(this.verifyEmailForm);
      return;
    }

    const verificationCode = this.verifyEmailForm.controls.code.value.trim();
    this.verifyRequestInFlight.set(true);

    this.authService
      .verifyEmail({
        email: this.verifyEmailAddress(),
        code: verificationCode,
      })
      .subscribe({
        next: () => {
          this.verifyRequestInFlight.set(false);
          this.userService.loadCurrentUser().subscribe({
            next: () => this.router.navigate(['/dashboard']),
            error: () => this.router.navigate(['/dashboard']),
          });
        },
        error: (error: HttpErrorResponse) => {
          this.verifyRequestInFlight.set(false);
          this.verifyEmailErrorMessage.set(this.resolveVerifyErrorMessage(error));
        },
      });
  }

  onResend(): void {
    if (this.resendCooldownSeconds() > 0 || this.resendRequestInFlight()) {
      return;
    }

    this.verifyEmailErrorMessage.set('');
    this.verifyEmailSuccessMessage.set('');
    this.resendRequestInFlight.set(true);

    this.authService.resendVerification({ email: this.verifyEmailAddress() }).subscribe({
      next: () => {
        this.resendRequestInFlight.set(false);
        this.verifyEmailSuccessMessage.set('A new verification code has been sent.');
        this.startResendCooldown(RESEND_COOLDOWN_SECONDS);
      },
      error: (error: HttpErrorResponse) => {
        this.resendRequestInFlight.set(false);
        this.verifyEmailErrorMessage.set(this.resolveResendErrorMessage(error));
      },
    });
  }

  private resolveVerifyErrorMessage(error: HttpErrorResponse): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return 'Unable to verify your email. Please try again.';
  }

  private resolveResendErrorMessage(error: HttpErrorResponse): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return 'Unable to resend the verification code. Please try again.';
  }

  private startResendCooldown(secondsRemaining: number): void {
    this.clearResendCooldownTimer();
    this.resendCooldownSeconds.set(secondsRemaining);
    this.resendCooldownLabel.set(this.formatResendCooldown(secondsRemaining));

    this.resendCooldownTimer = setInterval(() => {
      const nextSeconds = this.resendCooldownSeconds() - 1;
      if (nextSeconds <= 0) {
        this.resendCooldownSeconds.set(0);
        this.resendCooldownLabel.set('');
        this.clearResendCooldownTimer();
        return;
      }

      this.resendCooldownSeconds.set(nextSeconds);
      this.resendCooldownLabel.set(this.formatResendCooldown(nextSeconds));
    }, 1000);
  }

  private formatResendCooldown(secondsRemaining: number): string {
    const minutesRemaining = Math.floor(secondsRemaining / 60);
    const secondsPart = secondsRemaining % 60;

    if (minutesRemaining === 0) {
      return `${secondsPart}s`;
    }

    if (secondsPart === 0) {
      return `${minutesRemaining}m`;
    }

    return `${minutesRemaining}m ${secondsPart}s`;
  }

  private clearResendCooldownTimer(): void {
    if (!this.resendCooldownTimer) {
      return;
    }

    clearInterval(this.resendCooldownTimer);
    this.resendCooldownTimer = null;
  }
}
