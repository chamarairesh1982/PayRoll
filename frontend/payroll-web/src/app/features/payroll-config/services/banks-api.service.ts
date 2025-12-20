import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { Bank } from '../models/bank.model';

@Injectable({ providedIn: 'root' })
export class BanksApiService {
  private baseUrl = `${environment.apiBaseUrl}/banks`;

  constructor(private http: HttpClient) {}

  getBanks(params: {
    page?: number;
    pageSize?: number;
    search?: string | null;
    isActive?: boolean | null;
  }): Observable<PaginatedResult<Bank>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.isActive !== undefined && params.isActive !== null) {
      httpParams = httpParams.set('isActive', params.isActive);
    }

    return this.http.get<PaginatedResult<Bank>>(this.baseUrl, { params: httpParams });
  }

  getBank(id: string): Observable<Bank> {
    return this.http.get<Bank>(`${this.baseUrl}/${id}`);
  }

  createBank(payload: Partial<Bank>): Observable<Bank> {
    return this.http.post<Bank>(this.baseUrl, payload);
  }

  updateBank(id: string, payload: Partial<Bank>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteBank(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
