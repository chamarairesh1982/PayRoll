import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { LeaveRequest, LeaveStatus } from '../models/leave-request.model';

@Injectable({ providedIn: 'root' })
export class LeaveRequestsApiService {
  private baseUrl = `${environment.apiBaseUrl}/leave-requests`;

  constructor(private http: HttpClient) {}

  getLeaveRequests(params: {
    page?: number;
    pageSize?: number;
    employeeId?: string;
    status?: LeaveStatus | '';
  }): Observable<PaginatedResult<LeaveRequest>> {
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
    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    return this.http.get<PaginatedResult<LeaveRequest>>(this.baseUrl, { params: httpParams });
  }

  getLeaveRequest(id: string): Observable<LeaveRequest> {
    return this.http.get<LeaveRequest>(`${this.baseUrl}/${id}`);
  }

  createLeaveRequest(payload: Partial<LeaveRequest>): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(this.baseUrl, payload);
  }

  updateLeaveRequest(id: string, payload: Partial<LeaveRequest>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteLeaveRequest(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
