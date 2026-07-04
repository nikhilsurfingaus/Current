import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_CONFIG, ApiConfig } from '../config/api.config';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig,
  ) {}

  get<T>(path: string, headers?: HttpHeaders): Observable<T> {
    return this.http.get<T>(this.buildUrl(path), { headers });
  }

  post<T>(path: string, body: unknown, headers?: HttpHeaders): Observable<T> {
    return this.http.post<T>(this.buildUrl(path), body, { headers });
  }

  put<T>(path: string, body: unknown, headers?: HttpHeaders): Observable<T> {
    return this.http.put<T>(this.buildUrl(path), body, { headers });
  }

  delete<T>(path: string, headers?: HttpHeaders): Observable<T> {
    return this.http.delete<T>(this.buildUrl(path), { headers });
  }

  get apiUrl(): string {
    return this.apiConfig.apiUrl;
  }

  private buildUrl(path: string): string {
    return `${this.apiConfig.apiUrl}${path}`;
  }
}
