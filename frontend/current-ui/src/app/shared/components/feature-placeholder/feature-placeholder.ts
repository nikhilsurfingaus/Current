import { Component, input } from '@angular/core';

@Component({
  selector: 'app-feature-placeholder',
  standalone: true,
  template: `
    <h1 class="page-title">{{ pageTitle() }}</h1>
    <p class="page-subtitle">{{ pageSubtitle() }}</p>
  `,
  styles: `
    .page-title {
      margin-bottom: 8px;
      font-size: 28px;
      font-weight: 600;
      color: #f8fafc;
    }

    .page-subtitle {
      margin: 0;
      font-size: 14px;
      color: #94a3b8;
    }
  `,
})
export class FeaturePlaceholderComponent {
  pageTitle = input.required<string>();
  pageSubtitle = input.required<string>();
}
