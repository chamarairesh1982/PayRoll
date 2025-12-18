import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { RecurringRule, RecurringRuleSimulationRequest, RecurringRuleSimulationResult } from '../models/recurring-rule.model';

@Injectable({ providedIn: 'root' })
export class RecurringRulesApiService {
  private baseUrl = `${environment.apiBaseUrl}/recurring-rules`;

  constructor(private http: HttpClient) {}

  getRecurringRules(params: { page?: number; pageSize?: number; isActive?: boolean | null }): Observable<PaginatedResult<RecurringRule>> {
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

    return this.http.get<PaginatedResult<RecurringRule>>(this.baseUrl, { params: httpParams });
  }

  getRecurringRule(id: string): Observable<RecurringRule> {
    return this.http.get<RecurringRule>(`${this.baseUrl}/${id}`);
  }

  createRecurringRule(payload: Partial<RecurringRule>): Observable<RecurringRule> {
    return this.http.post<RecurringRule>(this.baseUrl, payload);
  }

  updateRecurringRule(id: string, payload: Partial<RecurringRule>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteRecurringRule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  simulate(payload: RecurringRuleSimulationRequest): Observable<RecurringRuleSimulationResult[]> {
    return this.http.post<RecurringRuleSimulationResult[]>(`${this.baseUrl}/simulate`, payload);
  }
}
