import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { Loan } from '../models/loan.model';

@Injectable({ providedIn: 'root' })
export class LoansApiService {
  private baseUrl = `${environment.apiBaseUrl}/loans`;

  constructor(private http: HttpClient) {}

  getLoans(params: { page?: number; pageSize?: number }): Observable<PaginatedResult<Loan>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    return this.http.get<PaginatedResult<Loan>>(this.baseUrl, { params: httpParams });
  }

  getLoan(id: string): Observable<Loan> {
    return this.http.get<Loan>(`${this.baseUrl}/${id}`);
  }

  createLoan(payload: Partial<Loan>): Observable<Loan> {
    return this.http.post<Loan>(this.baseUrl, payload);
  }

  repayLoan(id: string, amount: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/repay`, { amount });
  }
}
