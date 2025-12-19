import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  EpfEtfReportResult,
  FileExportResult,
  StatutoryReportHistory,
  StatutoryReportType,
} from '../models/statutory-report.model';

@Injectable({ providedIn: 'root' })
export class StatutoryReportsApiService {
  private baseUrl = `${environment.apiBaseUrl}/statutory-reports`;

  constructor(private http: HttpClient) {}

  getReports(params?: { type?: StatutoryReportType; payRunId?: string }): Observable<StatutoryReportHistory[]> {
    let httpParams = new HttpParams();

    if (params?.type) {
      httpParams = httpParams.set('type', params.type);
    }

    if (params?.payRunId) {
      httpParams = httpParams.set('payRunId', params.payRunId);
    }

    return this.http.get<StatutoryReportHistory[]>(this.baseUrl, { params: httpParams });
  }

  generateEpfEtfReport(payRunId: string, format: 'csv' | 'txt'): Observable<EpfEtfReportResult> {
    return this.http.post<EpfEtfReportResult>(`${this.baseUrl}/epf-etf`, { payRunId, format });
  }

  downloadReport(reportId: string, warnings = false): Observable<FileExportResult> {
    const params = warnings ? new HttpParams().set('warnings', 'true') : undefined;
    return this.http.get<FileExportResult>(`${this.baseUrl}/${reportId}/download`, { params });
  }
}
