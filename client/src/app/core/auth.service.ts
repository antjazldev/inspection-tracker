import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { AuthResponse } from './models';
import { environment } from '../../environments/environment';

const STORAGE_KEY = 'inspection-tracker-auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  private auth = signal<AuthResponse | null>(this.restore());

  readonly isLoggedIn = computed(() => this.auth() !== null);
  readonly displayName = computed(() => this.auth()?.displayName ?? '');
  readonly token = computed(() => this.auth()?.token ?? null);
  readonly userId = computed(() => {
    const token = this.token();
    if (!token) return null;
    try {
      return JSON.parse(atob(token.split('.')[1])).sub as string;
    } catch {
      return null;
    }
  });

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/api/auth/login`, { email, password })
      .pipe(tap((res) => this.store(res)));
  }

  register(email: string, displayName: string, password: string) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/api/auth/register`, { email, displayName, password })
      .pipe(tap((res) => this.store(res)));
  }

  logout(): void {
    this.auth.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private store(res: AuthResponse): void {
    this.auth.set(res);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(res));
  }

  private restore(): AuthResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
}