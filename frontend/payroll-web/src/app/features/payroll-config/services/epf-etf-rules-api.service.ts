import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { EpfEtfRuleSet } from '../models/epf-etf-rule-set.model';

@Injectable({ providedIn: 'root' })
export class EpfEtfRulesApiService {
  private baseUrl = `${environment.apiBaseUrl}/epf-etf-rules`;

  constructor(private http: HttpClient) {}

  getRuleSets(params: { page?: number; pageSize?: number; isActive?: boolean | null }): Observable<PaginatedResult<EpfEtfRuleSet>> {
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

    return this.http.get<PaginatedResult<EpfEtfRuleSet>>(this.baseUrl, { params: httpParams });
  }

  getRuleSet(id: string): Observable<EpfEtfRuleSet> {
    return this.http.get<EpfEtfRuleSet>(`${this.baseUrl}/${id}`);
  }

  createRuleSet(payload: Partial<EpfEtfRuleSet>): Observable<EpfEtfRuleSet> {
    const requestBody = {
      ...payload,
      effectiveFrom: payload.effectiveFrom ? new Date(payload.effectiveFrom).toISOString() : null,
      effectiveTo: payload.effectiveTo ? new Date(payload.effectiveTo).toISOString() : null,
    };

    return this.http.post<EpfEtfRuleSet>(this.baseUrl, requestBody);
  }

  updateRuleSet(id: string, payload: Partial<EpfEtfRuleSet>): Observable<void> {
    const requestBody = {
      ...payload,
      effectiveFrom: payload.effectiveFrom ? new Date(payload.effectiveFrom).toISOString() : null,
      effectiveTo: payload.effectiveTo ? new Date(payload.effectiveTo).toISOString() : null,
    };

    return this.http.put<void>(`${this.baseUrl}/${id}`, requestBody);
  }
}
