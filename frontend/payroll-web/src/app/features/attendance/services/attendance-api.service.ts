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

  getAttendanceRecords(params: { page?: number; pageSize?: number }): Observable<PaginatedResult<AttendanceRecord>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    return this.http.get<PaginatedResult<AttendanceRecord>>(this.baseUrl, { params: httpParams });
  }

  recordAttendance(payload: Partial<AttendanceRecord>): Observable<void> {
    return this.http.post<void>(this.baseUrl, payload);
  }
}
