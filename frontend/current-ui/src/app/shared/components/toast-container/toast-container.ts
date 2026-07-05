import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.scss',
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);

  dismissToast(toastId: string): void {
    this.toastService.dismiss(toastId);
  }
}
