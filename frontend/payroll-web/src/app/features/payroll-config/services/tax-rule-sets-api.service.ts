import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { TaxRuleSet, TaxSlab } from '../models/tax-rule-set.model';

@Injectable({ providedIn: 'root' })
export class TaxRuleSetsApiService {
  private baseUrl = `${environment.apiBaseUrl}/tax-rule-sets`;

  constructor(private http: HttpClient) {}

  getRuleSets(params: { page?: number; pageSize?: number; yearOfAssessment?: number | null }): Observable<PaginatedResult<TaxRuleSet>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }
    if (params.yearOfAssessment !== undefined && params.yearOfAssessment !== null) {
      httpParams = httpParams.set('yearOfAssessment', params.yearOfAssessment);
    }

    return this.http.get<PaginatedResult<TaxRuleSet>>(this.baseUrl, { params: httpParams });
  }

  getRuleSet(id: string): Observable<TaxRuleSet> {
    return this.http.get<TaxRuleSet>(`${this.baseUrl}/${id}`);
  }

  createRuleSet(payload: Partial<TaxRuleSet>): Observable<TaxRuleSet> {
    const requestBody = this.mapToRequest(payload);
    return this.http.post<TaxRuleSet>(this.baseUrl, requestBody);
  }

  updateRuleSet(id: string, payload: Partial<TaxRuleSet>): Observable<void> {
    const requestBody = this.mapToRequest(payload);
    return this.http.put<void>(`${this.baseUrl}/${id}`, requestBody);
  }

  private mapToRequest(payload: Partial<TaxRuleSet>): Partial<TaxRuleSet> {
    return {
      ...payload,
      effectiveFrom: payload.effectiveFrom ? new Date(payload.effectiveFrom).toISOString() : null,
      effectiveTo: payload.effectiveTo ? new Date(payload.effectiveTo).toISOString() : null,
      slabs:
        payload.slabs?.map((slab: TaxSlab) => ({
          id: slab.id,
          fromAmount: slab.fromAmount,
          toAmount: slab.toAmount ?? null,
          ratePercent: slab.ratePercent,
          order: slab.order,
        })) || [],
    };
  }
}
