import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserService } from '../../../core/services/user.service';
import { AuthMarkComponent } from '../../../shared/components/auth-mark/auth-mark';
import { ApiError } from '../../../shared/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthMarkComponent],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent implements OnInit {
  loginFormSubmitted = signal(false);
  loginErrorMessage = signal('');
  loginRequestInFlight = signal(false);
  passwordVisible = signal(false);

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
    private toastService: ToastService,
    private userService: UserService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.toastService.dismissAll();
  }

  togglePasswordVisible(): void {
    this.passwordVisible.update((visible) => !visible);
  }

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
        this.userService.loadCurrentUser().subscribe({
          next: () => this.router.navigate(['/dashboard']),
          error: () => this.router.navigate(['/dashboard']),
        });
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
