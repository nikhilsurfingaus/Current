import { AbstractControl, ValidationErrors } from '@angular/forms';

export function differentAccountsValidator(group: AbstractControl): ValidationErrors | null {
  const fromAccountId = group.get('fromAccountId')?.value;
  const toAccountId = group.get('toAccountId')?.value;

  if (fromAccountId && toAccountId && fromAccountId === toAccountId) {
    return { sameAccount: true };
  }

  return null;
}
