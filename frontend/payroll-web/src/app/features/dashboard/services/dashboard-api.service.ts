import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DashboardSummary, DashboardSummaryQuery } from '../models/dashboard-summary.model';

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private baseUrl = `${environment.apiBaseUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getSummary(query: DashboardSummaryQuery): Observable<DashboardSummary> {
    let params = new HttpParams();
    if (query.periodStart) {
      params = params.set('periodStart', query.periodStart);
    }
    if (query.periodEnd) {
      params = params.set('periodEnd', query.periodEnd);
    }
    if (query.companyId) {
      params = params.set('companyId', query.companyId);
    }
    if (query.branchId) {
      params = params.set('branchId', query.branchId);
    }
    if (query.costCenterId) {
      params = params.set('costCenterId', query.costCenterId);
    }

    return this.http.get<DashboardSummary>(`${this.baseUrl}/summary`, { params });
  }
}
