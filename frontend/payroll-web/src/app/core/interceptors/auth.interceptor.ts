import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();
    const roles = this.authService.getRoles();
    const username = this.authService.getUserName();
    if (token) {
      const authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
          ...(roles.length ? { 'X-User-Roles': roles.join(',') } : {}),
          ...(username ? { 'X-User-Name': username } : {}),
        },
      });
      return next.handle(authReq);
    }
    return next.handle(req);
  }
}
