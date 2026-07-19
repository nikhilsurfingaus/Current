import { Directive, ElementRef, HostListener, inject } from '@angular/core';

import { normalizeAmountInputValue } from '../utils/amount-input.utils';

@Directive({
  selector: 'input[type="number"][appNormalizeAmount]',
  standalone: true,
})
export class NormalizeAmountDirective {
  private readonly elementRef = inject(ElementRef<HTMLInputElement>);

  @HostListener('input')
  onInput(): void {
    // Run after Angular's number value accessor reads the raw keystroke.
    requestAnimationFrame(() => this.normalizeCurrentValue());
  }

  @HostListener('blur')
  onBlur(): void {
    this.normalizeCurrentValue();
  }

  private normalizeCurrentValue(): void {
    const input = this.elementRef.nativeElement;
    const rawValue = input.value;
    const normalizedValue = normalizeAmountInputValue(rawValue);

    if (normalizedValue === rawValue) {
      return;
    }

    const selectionStart = input.selectionStart ?? normalizedValue.length;
    const removedCharacterCount = rawValue.length - normalizedValue.length;
    const nextSelectionStart = Math.max(0, selectionStart - removedCharacterCount);

    input.value = normalizedValue;
    input.setSelectionRange(nextSelectionStart, nextSelectionStart);
  }
}
