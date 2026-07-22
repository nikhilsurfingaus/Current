import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="empty-state">
      <p class="empty-state__message">{{ message() }}</p>
      @if (actionLabel() && actionRoute()) {
        <a class="empty-state__action" [routerLink]="actionRoute()">{{ actionLabel() }}</a>
      }
    </div>
  `,
  styles: `
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 12px;
      padding: 8px 0;
    }

    .empty-state__message {
      margin: 0;
      font-size: 14px;
      color: var(--text-secondary);
    }

    .empty-state__action {
      display: inline-flex;
      align-items: center;
      min-height: 40px;
      padding: 8px 14px;
      border: 1px solid color-mix(in srgb, var(--color-primary) 30%, transparent);
      border-radius: 10px;
      background: var(--bg-secondary);
      color: var(--color-primary);
      font-size: 13px;
      font-weight: 600;
      text-decoration: none;

      &:hover {
        background: var(--bg-active);
      }

      &:focus-visible {
        outline: 2px solid var(--color-primary);
        outline-offset: 2px;
      }
    }
  `,
})
export class EmptyStateComponent {
  message = input.required<string>();
  actionLabel = input<string>('');
  actionRoute = input<string>('');
}
