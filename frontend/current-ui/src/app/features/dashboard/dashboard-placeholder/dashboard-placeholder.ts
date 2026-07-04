import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-placeholder',
  standalone: true,
  template: `
    <p class="placeholder">Dashboard — Part 4 (auth guard passed)</p>
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
export class DashboardPlaceholderComponent {}
