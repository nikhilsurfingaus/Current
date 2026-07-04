import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordMatchValidator(passwordControlName: string): ValidatorFn {
  return (confirmPasswordControl: AbstractControl): ValidationErrors | null => {
    const passwordControl = confirmPasswordControl.parent?.get(passwordControlName);
    if (!passwordControl) {
      return null;
    }

    if (confirmPasswordControl.value !== passwordControl.value) {
      return { passwordMismatch: true };
    }

    return null;
  };
}
