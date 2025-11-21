import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AttendanceRecord } from '../models/attendance-record.model';
import { PaginatedResult } from '../../employees/models/employee.model';

@Injectable({ providedIn: 'root' })
export class AttendanceApiService {
  private baseUrl = `${environment.apiBaseUrl}/attendance`;

  constructor(private http: HttpClient) {}

  getAttendanceRecords(params: { page?: number; pageSize?: number; date?: string; employeeId?: string }): Observable<PaginatedResult<AttendanceRecord>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }
    if (params.date) {
      httpParams = httpParams.set('date', params.date);
    }
    if (params.employeeId) {
      httpParams = httpParams.set('employeeId', params.employeeId);
    }

    return this.http.get<PaginatedResult<AttendanceRecord>>(this.baseUrl, { params: httpParams });
  }

  getAttendanceRecord(id: string): Observable<AttendanceRecord> {
    return this.http.get<AttendanceRecord>(`${this.baseUrl}/${id}`);
  }

  createAttendanceRecord(payload: Partial<AttendanceRecord>): Observable<AttendanceRecord> {
    return this.http.post<AttendanceRecord>(this.baseUrl, payload);
  }

  updateAttendanceRecord(id: string, payload: Partial<AttendanceRecord>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteAttendanceRecord(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
