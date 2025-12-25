import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, map } from 'rxjs';

interface LoginResponse {
    token: string;
    userId: string;
    tenantId: string;
}

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private currentUserSubject: BehaviorSubject<LoginResponse | null>;
    public currentUser: Observable<LoginResponse | null>;

    constructor(private http: HttpClient, private router: Router) {
        const storedUser = localStorage.getItem('currentUser');
        this.currentUserSubject = new BehaviorSubject<LoginResponse | null>(storedUser ? JSON.parse(storedUser) : null);
        this.currentUser = this.currentUserSubject.asObservable();
    }

    public get currentUserValue(): LoginResponse | null {
        return this.currentUserSubject.value;
    }

    login(email: string, password: string): Observable<LoginResponse> {
        return this.http.post<LoginResponse>('/api/auth/login', { email, password })
            .pipe(map(user => {
                // store user details and jwt token in local storage to keep user logged in between page refreshes
                localStorage.setItem('currentUser', JSON.stringify(user));
                localStorage.setItem('tenantId', user.tenantId); // Sync tenant ID
                this.currentUserSubject.next(user);
                return user;
            }));
    }

    logout() {
        // remove user from local storage to log user out
        localStorage.removeItem('currentUser');
        localStorage.removeItem('tenantId');
        this.currentUserSubject.next(null);
        this.router.navigate(['/login']);
    }
}
