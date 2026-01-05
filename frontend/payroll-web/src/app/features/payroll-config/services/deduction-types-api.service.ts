import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { DeductionType } from '../models/deduction-type.model';

@Injectable({ providedIn: 'root' })
export class DeductionTypesApiService {
  private baseUrl = `${environment.apiBaseUrl}/deduction-types`;

  constructor(private http: HttpClient) {}

  getDeductionTypes(params: {
    page?: number;
    pageSize?: number;
    isActive?: boolean | null;
  }): Observable<PaginatedResult<DeductionType>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }
    if (params.isActive !== undefined && params.isActive !== null) {
      httpParams = httpParams.set('isActive', params.isActive);
    }

    return this.http.get<PaginatedResult<DeductionType>>(this.baseUrl, { params: httpParams });
  }

  getDeductionType(id: string): Observable<DeductionType> {
    return this.http.get<DeductionType>(`${this.baseUrl}/${id}`);
  }

  createDeductionType(payload: Partial<DeductionType>): Observable<DeductionType> {
    return this.http.post<DeductionType>(this.baseUrl, payload);
  }

  updateDeductionType(id: string, payload: Partial<DeductionType>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteDeductionType(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
