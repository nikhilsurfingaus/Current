import { Component, OnInit, computed, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';

import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import {
  getDisplayNameFromEmail,
  getTimeGreeting,
  getUserInitialsFromNames,
} from '../../shared/utils/user-display.utils';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private router = inject(Router);

  greetingLabel = computed(() => {
    const currentUser = this.userService.currentUser();
    const userEmail = this.authService.getAuthResponse()?.email ?? '';
    const greetingName = currentUser?.firstName
      ?? (userEmail ? getDisplayNameFromEmail(userEmail) : 'there');

    return `${getTimeGreeting()}, ${greetingName}`;
  });

  userInitials = computed(() => {
    const currentUser = this.userService.currentUser();

    if (currentUser) {
      return getUserInitialsFromNames(currentUser.firstName, currentUser.lastName);
    }

    return '?';
  });

  userDisplayName = computed(() => {
    const currentUser = this.userService.currentUser();

    if (currentUser) {
      return `${currentUser.firstName} ${currentUser.lastName}`;
    }

    const userEmail = this.authService.getAuthResponse()?.email ?? '';
    return userEmail ? getDisplayNameFromEmail(userEmail) : 'User';
  });

  userEmail = computed(() => this.authService.getAuthResponse()?.email ?? '');

  private currentUrl = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map((event) => (event as NavigationEnd).urlAfterRedirects),
      startWith(this.router.url),
    ),
    { initialValue: this.router.url },
  );

  isDashboardRoute = computed(() => this.currentUrl().startsWith('/dashboard'));

  ngOnInit(): void {
    this.authService.initializeSession();
    this.userService.loadCurrentUser().subscribe();
  }

  get currentYear(): number {
    return new Date().getFullYear();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
