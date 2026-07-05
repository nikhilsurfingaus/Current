import { Component, computed, input } from '@angular/core';

import {
  DEFAULT_GOAL_ICON_KEY,
  resolveGoalIconOption,
} from '../../constants/goal-icon-options';

@Component({
  selector: 'app-goal-icon',
  standalone: true,
  template: `
    <span
      class="goal-icon"
      [style.background]="iconOption().backgroundColor"
      [style.color]="iconOption().iconColor"
      aria-hidden="true"
    >
      @switch (iconOption().key) {
        @case ('vacation') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <circle cx="12" cy="12" r="4" stroke="currentColor" stroke-width="1.8" />
            <path
              d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M5.6 18.4l1.4-1.4M17 7l1.4-1.4"
              stroke="currentColor"
              stroke-width="1.8"
              stroke-linecap="round"
            />
          </svg>
        }
        @case ('home') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6H10v6H5a1 1 0 0 1-1-1v-9.5Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" />
          </svg>
        }
        @case ('emergency') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M12 3 4 20h16L12 3Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" />
            <path d="M12 10v4M12 17h.01" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
          </svg>
        }
        @case ('car') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M5 16h14M6 16l1.5-5h9L18 16M7 19a1 1 0 1 0 0-2 1 1 0 0 0 0 2ZM17 19a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
          </svg>
        }
        @case ('gaming') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <rect x="4" y="8" width="16" height="8" rx="3" stroke="currentColor" stroke-width="1.8" />
            <path d="M9 12h2M10 11v2M15 11.5h.01M17 13.5h.01" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
          </svg>
        }
        @case ('investment') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M4 18V6M8 18V10M12 18V14M16 18V8M20 18V4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
          </svg>
        }
        @case ('education') {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M12 4 3 8l9 4 9-4-9-4Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" />
            <path d="M6 10v5c0 1.7 2.7 3 6 3s6-1.3 6-3v-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
          </svg>
        }
        @default {
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <rect x="3" y="6" width="18" height="13" rx="2" stroke="currentColor" stroke-width="1.8" />
            <path d="M3 10h18M8 14h2" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
          </svg>
        }
      }
    </span>
  `,
  styles: `
    .goal-icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: 12px;
      flex-shrink: 0;
    }
  `,
})
export class GoalIconComponent {
  iconKey = input<string>(DEFAULT_GOAL_ICON_KEY);

  iconOption = computed(() => resolveGoalIconOption(this.iconKey()));
}
