import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { OvertimeRecord, OvertimeStatus } from '../models/overtime-record.model';

@Injectable({ providedIn: 'root' })
export class OvertimeApiService {
  private baseUrl = `${environment.apiBaseUrl}/overtime`;

  constructor(private http: HttpClient) {}

  getOvertimeRecords(params: {
    page?: number;
    pageSize?: number;
    employeeId?: string;
    date?: string;
    status?: OvertimeStatus | '';
  }): Observable<PaginatedResult<OvertimeRecord>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }

    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    if (params.employeeId) {
      httpParams = httpParams.set('employeeId', params.employeeId);
    }

    if (params.date) {
      httpParams = httpParams.set('date', params.date);
    }

    if (params.status !== undefined) {
      httpParams = httpParams.set('status', params.status ?? '');
    }

    return this.http.get<PaginatedResult<OvertimeRecord>>(this.baseUrl, { params: httpParams });
  }

  getOvertimeRecord(id: string): Observable<OvertimeRecord> {
    return this.http.get<OvertimeRecord>(`${this.baseUrl}/${id}`);
  }

  createOvertimeRecord(payload: Partial<OvertimeRecord>): Observable<OvertimeRecord> {
    return this.http.post<OvertimeRecord>(this.baseUrl, payload);
  }

  updateOvertimeRecord(id: string, payload: Partial<OvertimeRecord>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteOvertimeRecord(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
