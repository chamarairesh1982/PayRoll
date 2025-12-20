import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { OTEntry, OvertimeStatus } from '../models/ot-entry.model';

@Injectable({ providedIn: 'root' })
export class OvertimeApiService {
  private baseUrl = `${environment.apiBaseUrl}/overtime/entries`;

  constructor(private http: HttpClient) {}

  getOvertimeRecords(params: {
    page?: number;
    pageSize?: number;
    employeeId?: string;
    from?: string;
    to?: string;
    status?: OvertimeStatus | '';
  }): Observable<PaginatedResult<OTEntry>> {
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

    if (params.from) {
      httpParams = httpParams.set('from', params.from);
    }

    if (params.to) {
      httpParams = httpParams.set('to', params.to);
    }

    if (params.status !== undefined) {
      httpParams = httpParams.set('status', params.status ?? '');
    }

    return this.http.get<PaginatedResult<OTEntry>>(this.baseUrl, { params: httpParams });
  }

  getOvertimeRecord(id: string): Observable<OTEntry> {
    return this.http.get<OTEntry>(`${this.baseUrl}/${id}`);
  }

  createOvertimeRecord(payload: Partial<OTEntry>): Observable<OTEntry> {
    return this.http.post<OTEntry>(this.baseUrl, payload);
  }

  updateOvertimeRecord(id: string, payload: Partial<OTEntry>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  submitOvertimeRecord(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/submit`, {});
  }

  approveOvertimeRecord(id: string, comment?: string | null): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/approve`, { comment });
  }

  rejectOvertimeRecord(id: string, comment?: string | null): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reject`, { comment });
  }
}
