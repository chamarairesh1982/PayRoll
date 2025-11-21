import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { PayPeriodType, PayRunDetail, PayRunStatus, PayRunSummary } from '../models/pay-run.model';
import { PaySlip } from '../models/payslip.model';

@Injectable({ providedIn: 'root' })
export class PayRunsApiService {
  private baseUrl = `${environment.apiBaseUrl}/payruns`;

  constructor(private http: HttpClient) {}

  getPayRuns(params: { page?: number; pageSize?: number; status?: PayRunStatus | '' }): Observable<PaginatedResult<PayRunSummary>> {
    let httpParams = new HttpParams();

    if (params.page !== undefined) {
      httpParams = httpParams.set('page', params.page);
    }

    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }

    return this.http.get<PaginatedResult<PayRunSummary>>(this.baseUrl, { params: httpParams });
  }

  getPayRun(id: string): Observable<PayRunDetail> {
    return this.http.get<PayRunDetail>(`${this.baseUrl}/${id}`);
  }

  createPayRun(payload: {
    name: string;
    periodType: PayPeriodType;
    periodStart: string;
    periodEnd: string;
    payDate: string;
    includeActiveEmployeesOnly: boolean;
    employeeIds?: string[];
  }): Observable<PayRunDetail> {
    return this.http.post<PayRunDetail>(this.baseUrl, payload);
  }

  recalculatePayRun(
    id: string,
    payload: {
      includeAttendance: boolean;
      includeOvertime: boolean;
      includeLoans: boolean;
      includeAllowancesAndDeductions: boolean;
    },
  ): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/recalculate`, payload);
  }

  changeStatus(id: string, status: PayRunStatus): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/status`, { status });
  }

  getPaySlip(payRunId: string, paySlipId: string): Observable<PaySlip> {
    return this.http.get<PaySlip>(`${this.baseUrl}/${payRunId}/payslips/${paySlipId}`);
  }
}
