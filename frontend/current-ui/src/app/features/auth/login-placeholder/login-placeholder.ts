import { Component } from '@angular/core';

@Component({
  selector: 'app-login-placeholder',
  standalone: true,
  template: `
    <p class="placeholder">Login page — Part 5</p>
  `,
  styles: `
    .placeholder {
      margin: 0;
      color: #94a3b8;
      font-size: 14px;
      text-align: center;
    }
  `,
})
export class LoginPlaceholderComponent {}
