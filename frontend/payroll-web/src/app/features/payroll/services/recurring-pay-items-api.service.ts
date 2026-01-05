import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import {
  RecurringPayItemAssignment,
  RecurringPayItemRule,
  RecurringPayItemSimulationResponse,
} from '../models/recurring-pay-item.model';

@Injectable({ providedIn: 'root' })
export class RecurringPayItemsApiService {
  private baseUrl = `${environment.apiBaseUrl}/recurring-pay-items`;

  constructor(private http: HttpClient) {}

  getRules(params: { page?: number; pageSize?: number; activeOnly?: boolean }): Observable<PaginatedResult<RecurringPayItemRule>> {
    const httpParams = new URLSearchParams();
    if (params.page) {
      httpParams.set('page', String(params.page));
    }
    if (params.pageSize) {
      httpParams.set('pageSize', String(params.pageSize));
    }
    if (params.activeOnly !== undefined) {
      httpParams.set('activeOnly', String(params.activeOnly));
    }
    return this.http.get<PaginatedResult<RecurringPayItemRule>>(`${this.baseUrl}/rules?${httpParams.toString()}`);
  }

  getRule(id: string): Observable<RecurringPayItemRule> {
    return this.http.get<RecurringPayItemRule>(`${this.baseUrl}/rules/${id}`);
  }

  createRule(payload: Partial<RecurringPayItemRule>): Observable<RecurringPayItemRule> {
    return this.http.post<RecurringPayItemRule>(`${this.baseUrl}/rules`, payload);
  }

  updateRule(id: string, payload: Partial<RecurringPayItemRule>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/rules/${id}`, payload);
  }

  deleteRule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/rules/${id}`);
  }

  getAssignments(params: { employeeId?: string; ruleId?: string; activeOnly?: boolean }): Observable<RecurringPayItemAssignment[]> {
    const httpParams = new URLSearchParams();
    if (params.employeeId) {
      httpParams.set('employeeId', params.employeeId);
    }
    if (params.ruleId) {
      httpParams.set('ruleId', params.ruleId);
    }
    if (params.activeOnly !== undefined) {
      httpParams.set('activeOnly', String(params.activeOnly));
    }
    return this.http.get<RecurringPayItemAssignment[]>(`${this.baseUrl}/assignments?${httpParams.toString()}`);
  }

  createAssignments(payload: {
    ruleId: string;
    employeeIds: string[];
    startDate: string;
    endDate?: string | null;
    isActive: boolean;
  }): Observable<RecurringPayItemAssignment[]> {
    return this.http.post<RecurringPayItemAssignment[]>(`${this.baseUrl}/assignments`, payload);
  }

  updateAssignment(id: string, payload: { startDate: string; endDate?: string | null; isActive: boolean }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/assignments/${id}`, payload);
  }

  deleteAssignment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/assignments/${id}`);
  }

  simulate(payload: { employeeId: string; periodStart: string; periodEnd: string }): Observable<RecurringPayItemSimulationResponse> {
    return this.http.post<RecurringPayItemSimulationResponse>(`${this.baseUrl}/simulate`, payload);
  }
}
