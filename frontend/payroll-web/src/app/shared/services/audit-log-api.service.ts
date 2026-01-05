import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult } from '../../features/employees/models/employee.model';
import { AuditLogEntry } from '../models/audit-log.model';

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
  private baseUrl = `${environment.apiBaseUrl}/audit`;

  constructor(private http: HttpClient) {}

  getAuditLogs(params: {
    page?: number;
    pageSize?: number;
    entityType?: string;
    entityId?: string;
    action?: string;
    actor?: string;
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

    if (params.entityType) {
      httpParams = httpParams.set('entityType', params.entityType);
    }

    if (params.entityId) {
      httpParams = httpParams.set('entityId', params.entityId);
    }

    if (params.action) {
      httpParams = httpParams.set('action', params.action);
    }

    if (params.actor) {
      httpParams = httpParams.set('actor', params.actor);
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
    entityType?: string;
    entityId?: string;
    action?: string;
    actor?: string;
    from?: string;
    to?: string;
  }): Observable<Blob> {
    let httpParams = new HttpParams().set('export', 'true');

    if (params.entityType) {
      httpParams = httpParams.set('entityType', params.entityType);
    }

    if (params.entityId) {
      httpParams = httpParams.set('entityId', params.entityId);
    }

    if (params.action) {
      httpParams = httpParams.set('action', params.action);
    }

    if (params.actor) {
      httpParams = httpParams.set('actor', params.actor);
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
