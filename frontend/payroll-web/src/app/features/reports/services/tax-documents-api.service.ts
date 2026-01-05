import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { FileExportResult } from '../models/statutory-report.model';
import {
  AnnualTaxReportRequest,
  MonthlyTaxReportRequest,
  TaxCertificateRequest,
  TaxDocumentHistory,
  TaxDocumentMetadata,
} from '../models/tax-document.model';

@Injectable({ providedIn: 'root' })
export class TaxDocumentsApiService {
  private baseUrl = `${environment.apiBaseUrl}/tax`;

  constructor(private http: HttpClient) {}

  generateMonthlyReport(request: MonthlyTaxReportRequest, regenerate = false): Observable<TaxDocumentMetadata> {
    const params = regenerate ? new HttpParams().set('regenerate', 'true') : undefined;
    return this.http.post<TaxDocumentMetadata>(`${this.baseUrl}/reports/monthly`, request, { params });
  }

  generateAnnualReport(request: AnnualTaxReportRequest, regenerate = false): Observable<TaxDocumentMetadata> {
    const params = regenerate ? new HttpParams().set('regenerate', 'true') : undefined;
    return this.http.post<TaxDocumentMetadata>(`${this.baseUrl}/reports/annual`, request, { params });
  }

  generateCertificate(request: TaxCertificateRequest, regenerate = false): Observable<TaxDocumentMetadata> {
    const params = regenerate ? new HttpParams().set('regenerate', 'true') : undefined;
    return this.http.post<TaxDocumentMetadata>(`${this.baseUrl}/certificates`, request, { params });
  }

  getDocuments(params?: { type?: string; year?: number; employeeId?: string }): Observable<TaxDocumentHistory[]> {
    let httpParams = new HttpParams();
    if (params?.type) {
      httpParams = httpParams.set('type', params.type);
    }
    if (params?.year) {
      httpParams = httpParams.set('year', String(params.year));
    }
    if (params?.employeeId) {
      httpParams = httpParams.set('employeeId', params.employeeId);
    }
    return this.http.get<TaxDocumentHistory[]>(`${this.baseUrl}/documents`, { params: httpParams });
  }

  downloadDocument(documentId: string): Observable<FileExportResult> {
    return this.http.get<FileExportResult>(`${this.baseUrl}/documents/${documentId}/download`);
  }
}
