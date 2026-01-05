import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { BankBranch } from '../models/bank-branch.model';

@Injectable({ providedIn: 'root' })
export class BankBranchesApiService {
  private baseUrl = `${environment.apiBaseUrl}/bank-branches`;

  constructor(private http: HttpClient) {}

  getBankBranches(params: {
    page?: number;
    pageSize?: number;
    bankId?: string | null;
    search?: string | null;
    isActive?: boolean | null;
  }): Observable<PaginatedResult<BankBranch>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }
    if (params.bankId) {
      httpParams = httpParams.set('bankId', params.bankId);
    }
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.isActive !== undefined && params.isActive !== null) {
      httpParams = httpParams.set('isActive', params.isActive);
    }

    return this.http.get<PaginatedResult<BankBranch>>(this.baseUrl, { params: httpParams });
  }

  getBankBranch(id: string): Observable<BankBranch> {
    return this.http.get<BankBranch>(`${this.baseUrl}/${id}`);
  }

  createBankBranch(payload: Partial<BankBranch>): Observable<BankBranch> {
    return this.http.post<BankBranch>(this.baseUrl, payload);
  }

  updateBankBranch(id: string, payload: Partial<BankBranch>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteBankBranch(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
