import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../../features/employees/models/employee.model';
import { AuditLogEntry } from '../models/audit-log.model';

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
  private baseUrl = `${environment.apiBaseUrl}/audit-logs`;

  constructor(private http: HttpClient) {}

  getAuditLogs(params: {
    page?: number;
    pageSize?: number;
    entityName?: string;
    entityId?: string;
    action?: string;
    createdBy?: string;
    from?: string;
    to?: string;
  }): Observable<PaginatedResult<AuditLogEntry>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }

    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    if (params.entityName) {
      httpParams = httpParams.set('entityName', params.entityName);
    }

    if (params.entityId) {
      httpParams = httpParams.set('entityId', params.entityId);
    }

    if (params.action) {
      httpParams = httpParams.set('action', params.action);
    }

    if (params.createdBy) {
      httpParams = httpParams.set('createdBy', params.createdBy);
    }

    if (params.from) {
      httpParams = httpParams.set('from', params.from);
    }

    if (params.to) {
      httpParams = httpParams.set('to', params.to);
    }

    return this.http.get<PaginatedResult<AuditLogEntry>>(this.baseUrl, { params: httpParams });
  }

  exportAuditLogs(params: {
    entityName?: string;
    entityId?: string;
    action?: string;
    createdBy?: string;
    from?: string;
    to?: string;
  }): Observable<Blob> {
    let httpParams = new HttpParams().set('export', 'true');

    if (params.entityName) {
      httpParams = httpParams.set('entityName', params.entityName);
    }

    if (params.entityId) {
      httpParams = httpParams.set('entityId', params.entityId);
    }

    if (params.action) {
      httpParams = httpParams.set('action', params.action);
    }

    if (params.createdBy) {
      httpParams = httpParams.set('createdBy', params.createdBy);
    }

    if (params.from) {
      httpParams = httpParams.set('from', params.from);
    }

    if (params.to) {
      httpParams = httpParams.set('to', params.to);
    }

    return this.http.get(this.baseUrl, { params: httpParams, responseType: 'blob' });
  }
}
