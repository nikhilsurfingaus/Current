import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import { User } from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private currentUserState = signal<User | null>(null);

  readonly currentUser = this.currentUserState.asReadonly();

  constructor(private apiService: ApiService) {}

  loadCurrentUser(): Observable<User> {
    return this.apiService.get<User>(API_PATHS.users.me).pipe(
      tap((user) => this.currentUserState.set(user)),
    );
  }

  clearCurrentUser(): void {
    this.currentUserState.set(null);
  }
}
