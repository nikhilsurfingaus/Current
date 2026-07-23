import {
  Component,
  ElementRef,
  HostListener,
  OnInit,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { NotificationBellComponent } from '../../shared/components/notification-bell/notification-bell';
import { UserRole } from '../../shared/models';
import {
  getDisplayNameFromEmail,
  getTimeGreeting,
  getUserInitialsFromNames,
} from '../../shared/utils/user-display.utils';

const MOBILE_NAV_BREAKPOINT_PX = 900;

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NotificationBellComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private router = inject(Router);

  private menuButton = viewChild<ElementRef<HTMLButtonElement>>('menuButton');
  private firstNavLink = viewChild<ElementRef<HTMLAnchorElement>>('firstNavLink');

  mobileNavOpen = signal(false);
  isMobileViewport = signal(
    typeof window !== 'undefined' && window.innerWidth <= MOBILE_NAV_BREAKPOINT_PX,
  );

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

  isAdminSlimNav = computed(() => this.isAdminUser());

  isAdminUser = computed(
    () => this.authService.getAuthResponse()?.role === UserRole.Admin,
  );

  constructor() {
    effect((onCleanup) => {
      const navOpen = this.mobileNavOpen();
      document.body.style.overflow = navOpen ? 'hidden' : '';

      onCleanup(() => {
        document.body.style.overflow = '';
      });
    });

    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.closeMobileNav());
  }

  ngOnInit(): void {
    this.authService.initializeSession();
    this.userService.loadCurrentUser().subscribe();
  }

  get currentYear(): number {
    return new Date().getFullYear();
  }

  toggleMobileNav(): void {
    if (this.mobileNavOpen()) {
      this.closeMobileNav(true);
      return;
    }

    this.openMobileNav();
  }

  openMobileNav(): void {
    this.mobileNavOpen.set(true);
    queueMicrotask(() => this.firstNavLink()?.nativeElement.focus());
  }

  closeMobileNav(restoreMenuFocus = false): void {
    if (!this.mobileNavOpen()) {
      return;
    }

    this.mobileNavOpen.set(false);

    if (restoreMenuFocus) {
      queueMicrotask(() => this.menuButton()?.nativeElement.focus());
    }
  }

  logout(): void {
    this.closeMobileNav();
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.mobileNavOpen()) {
      this.closeMobileNav(true);
    }
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    const isMobile = window.innerWidth <= MOBILE_NAV_BREAKPOINT_PX;
    this.isMobileViewport.set(isMobile);

    if (!isMobile && this.mobileNavOpen()) {
      this.closeMobileNav();
    }
  }
}
