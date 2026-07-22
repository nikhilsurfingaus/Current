import { Component, input } from '@angular/core';

@Component({
  selector: 'app-skeleton-loader',
  standalone: true,
  template: `
    <div
      class="skeleton-loader"
      [class.skeleton-loader--text]="variant() === 'text'"
      [class.skeleton-loader--block]="variant() === 'block'"
      [style.width]="width()"
      [style.height]="height()"
      aria-hidden="true"
    ></div>
  `,
  styles: `
    .skeleton-loader {
      display: block;
      border-radius: 8px;
      background: linear-gradient(
        90deg,
        var(--bg-elevated) 0%,
        color-mix(in srgb, var(--bg-elevated) 70%, var(--text-secondary)) 50%,
        var(--bg-elevated) 100%
      );
      background-size: 200% 100%;
      animation: skeleton-shimmer 1.2s ease-in-out infinite;
    }

    .skeleton-loader--text {
      height: 14px;
      width: 100%;
    }

    .skeleton-loader--block {
      min-height: 80px;
      width: 100%;
    }

    @keyframes skeleton-shimmer {
      0% {
        background-position: 200% 0;
      }

      100% {
        background-position: -200% 0;
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .skeleton-loader {
        animation: none;
      }
    }
  `,
})
export class SkeletonLoaderComponent {
  variant = input<'text' | 'block'>('text');
  width = input<string>('100%');
  height = input<string | null>(null);
}
