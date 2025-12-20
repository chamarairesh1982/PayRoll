import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

const TOKEN_KEY = 'payroll_auth_token';
const USERNAME_KEY = 'payroll_auth_user';
const ROLES_KEY = 'payroll_auth_roles';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  login(username: string, _password: string): void {
    const fakeToken = btoa(`${username}-token`);
    localStorage.setItem(TOKEN_KEY, fakeToken);
    localStorage.setItem(USERNAME_KEY, username);
    const roles = username.trim().toLowerCase() === 'admin' ? ['Admin'] : [];
    localStorage.setItem(ROLES_KEY, JSON.stringify(roles));
    this.isAuthenticatedSubject.next(true);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USERNAME_KEY);
    localStorage.removeItem(ROLES_KEY);
    this.isAuthenticatedSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUserName(): string | null {
    return localStorage.getItem(USERNAME_KEY);
  }

  getRoles(): string[] {
    const raw = localStorage.getItem(ROLES_KEY);
    if (!raw) {
      return [];
    }

    try {
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  isAdmin(): boolean {
    return this.getRoles().some(role => role.toLowerCase() === 'admin');
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
  }
}
