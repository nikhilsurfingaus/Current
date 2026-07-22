import { FormGroup } from '@angular/forms';

export function focusFirstInvalidControl(formGroup: FormGroup): void {
  const firstInvalidControlName = Object.keys(formGroup.controls).find((controlName) => {
    const control = formGroup.get(controlName);
    return control?.invalid;
  });

  if (!firstInvalidControlName) {
    return;
  }

  const invalidElement = document.getElementById(getControlElementId(firstInvalidControlName));
  invalidElement?.focus();
}

function getControlElementId(controlName: string): string {
  const controlElementIds: Record<string, string> = {
    name: 'account-name',
    email: 'login-email',
    password: 'login-password',
    amount: 'transfer-amount',
    targetAmount: 'goal-target',
    currentBalance: 'account-balance',
    currency: 'account-currency',
    recipientEmail: 'payment-recipient-email',
    fromAccountId: 'transfer-from',
    toAccountId: 'transfer-to',
    firstName: 'settings-first-name',
    lastName: 'settings-last-name',
    themePreference: 'settings-theme',
    preferredCurrency: 'settings-currency',
    timezone: 'settings-timezone',
    locale: 'settings-locale',
    goalName: 'goal-name',
  };

  return controlElementIds[controlName] ?? controlName;
}

export function getControlDescribedBy(
  controlName: string,
  options: { invalid: boolean; hintId?: string },
): string | null {
  const describedByIds: string[] = [];

  if (options.hintId) {
    describedByIds.push(options.hintId);
  }

  if (options.invalid) {
    describedByIds.push(`${controlName}-error`);
  }

  return describedByIds.length > 0 ? describedByIds.join(' ') : null;
}
