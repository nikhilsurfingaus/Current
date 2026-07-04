import { Component } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../core/services/auth.service';
import { ApiError } from '../../../shared/models';
import { passwordMatchValidator } from './password-match.validator';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class RegisterComponent {
  registerFormSubmitted = false;
  registerErrorMessage = '';
  registerRequestInFlight = false;

  registerForm = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
    confirmPassword: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, passwordMatchValidator('password')],
    }),
  });

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  onSubmit(): void {
    this.registerFormSubmitted = true;
    this.registerErrorMessage = '';

    if (this.registerForm.invalid) {
      return;
    }

    const registerFormValues = this.registerForm.getRawValue();
    const registerRequest = {
      firstName: registerFormValues.firstName,
      lastName: registerFormValues.lastName,
      email: registerFormValues.email,
      password: registerFormValues.password,
    };

    this.registerRequestInFlight = true;

    this.authService.register(registerRequest).subscribe({
      next: () => {
        this.registerRequestInFlight = false;
        this.router.navigate(['/dashboard']);
      },
      error: (error: HttpErrorResponse) => {
        this.registerRequestInFlight = false;
        this.registerErrorMessage = this.resolveRegisterErrorMessage(error);
      },
    });
  }

  private resolveRegisterErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return 'An account with this email already exists.';
    }

    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return 'Unable to register. Please try again.';
  }
}
