import { Component } from '@angular/core';

@Component({
  selector: 'app-auth-mark',
  standalone: true,
  template: `
    <span class="auth-mark" aria-hidden="true">
      <svg class="auth-mark__svg" viewBox="0 0 48 36" fill="none">
        <path
          d="M4 14c8-8 20-8 28 0s20 8 28 0"
          stroke="currentColor"
          stroke-width="3.5"
          stroke-linecap="round"
        />
        <path
          d="M4 24c8-8 20-8 28 0s20 8 28 0"
          stroke="currentColor"
          stroke-width="3.5"
          stroke-linecap="round"
          opacity="0.55"
        />
      </svg>
    </span>
  `,
  styles: `
    .auth-mark {
      display: flex;
      justify-content: center;
      margin-bottom: 20px;
      color: #2f80ed;
    }

    .auth-mark__svg {
      width: 40px;
      height: 30px;
    }
  `,
})
export class AuthMarkComponent {}
