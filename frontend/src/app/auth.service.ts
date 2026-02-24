import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { AuthResponse, UserProfile } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'hfinance_token';
  private readonly userKey = 'hfinance_user';

  readonly user = signal<UserProfile | null>(this.loadUser());

  constructor(private readonly httpClient: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.httpClient
      .post<AuthResponse>(`${API_BASE_URL}/auth/login`, { email, password })
      .pipe(tap((response) => this.persistSession(response)));
  }

  register(email: string, password: string, displayName: string): Observable<AuthResponse> {
    return this.httpClient
      .post<AuthResponse>(`${API_BASE_URL}/auth/register`, { email, password, displayName })
      .pipe(tap((response) => this.persistSession(response)));
  }

  getMe(): Observable<UserProfile> {
    return this.httpClient.get<UserProfile>(`${API_BASE_URL}/account/me`).pipe(
      tap((profile) => {
        this.user.set(profile);
        localStorage.setItem(this.userKey, JSON.stringify(profile));
      })
    );
  }

  updateProfile(email: string, displayName: string, baseCurrency: string): Observable<UserProfile> {
    return this.httpClient
      .put<UserProfile>(`${API_BASE_URL}/account/me`, { email, displayName, baseCurrency })
      .pipe(
        tap((profile) => {
          this.user.set(profile);
          localStorage.setItem(this.userKey, JSON.stringify(profile));
        })
      );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.httpClient.put<void>(`${API_BASE_URL}/account/password`, { currentPassword, newPassword });
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.user()?.roles.includes('Admin') ?? false;
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.user.set(null);
  }

  private persistSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.accessToken);
    localStorage.setItem(this.userKey, JSON.stringify(response.user));
    this.user.set(response.user);
  }

  private loadUser(): UserProfile | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as UserProfile;
    } catch {
      return null;
    }
  }
}
