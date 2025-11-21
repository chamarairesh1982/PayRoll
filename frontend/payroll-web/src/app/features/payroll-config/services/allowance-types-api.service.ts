import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { AllowanceType } from '../models/allowance-type.model';

@Injectable({ providedIn: 'root' })
export class AllowanceTypesApiService {
  private baseUrl = `${environment.apiBaseUrl}/allowance-types`;

  constructor(private http: HttpClient) {}

  getAllowanceTypes(params: {
    page?: number;
    pageSize?: number;
    isActive?: boolean | null;
  }): Observable<PaginatedResult<AllowanceType>> {
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

    return this.http.get<PaginatedResult<AllowanceType>>(this.baseUrl, { params: httpParams });
  }

  getAllowanceType(id: string): Observable<AllowanceType> {
    return this.http.get<AllowanceType>(`${this.baseUrl}/${id}`);
  }

  createAllowanceType(payload: Partial<AllowanceType>): Observable<AllowanceType> {
    return this.http.post<AllowanceType>(this.baseUrl, payload);
  }

  updateAllowanceType(id: string, payload: Partial<AllowanceType>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteAllowanceType(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
