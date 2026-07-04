import { Component, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../core/services/auth.service';
import { ApiError } from '../../../shared/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  loginFormSubmitted = signal(false);
  loginErrorMessage = signal('');
  loginRequestInFlight = signal(false);

  loginForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
  });

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  onSubmit(): void {
    this.loginFormSubmitted.set(true);
    this.loginErrorMessage.set('');

    if (this.loginForm.invalid) {
      return;
    }

    const loginRequest = this.loginForm.getRawValue();
    this.loginRequestInFlight.set(true);

    this.authService.login(loginRequest).subscribe({
      next: () => {
        this.loginRequestInFlight.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (error: HttpErrorResponse) => {
        this.loginRequestInFlight.set(false);
        this.loginErrorMessage.set(this.resolveLoginErrorMessage(error));
      },
    });
  }

  private resolveLoginErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 401) {
      return 'Invalid email or password.';
    }

    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return 'Unable to log in. Please try again.';
  }
}
