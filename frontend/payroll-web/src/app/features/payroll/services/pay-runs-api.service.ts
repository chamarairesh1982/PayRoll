import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaginatedResult } from '../../employees/models/employee.model';
import { ApitReport, FileExportResult } from '../models/apit-report.model';
import {
  BankExportGenerateResult,
  BankExportTemplate,
  PayRunBankExport,
} from '../models/bank-export.model';
import { PayslipBulkGenerateResult, PayslipDocument } from '../models/payslip-document.model';
import { PayPeriodType, PayRunDetail, PayRunStatus, PayRunSummary } from '../models/pay-run.model';
import { PaySlip } from '../models/payslip.model';

export interface PayRunActionRequest {
  comment?: string;
}

@Injectable({ providedIn: 'root' })
export class PayRunsApiService {
  private baseUrl = `${environment.apiBaseUrl}/payruns`;
  private bankExportUrl = `${environment.apiBaseUrl}/bank-exports`;

  constructor(private http: HttpClient) {}

  getPayRuns(params: {
    page?: number;
    pageSize?: number;
    status?: PayRunStatus | '';
    companyId?: string;
    branchId?: string;
    costCenterId?: string;
    isConsolidated?: boolean;
  }): Observable<PaginatedResult<PayRunSummary>> {
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

    if (params.companyId) {
      httpParams = httpParams.set('companyId', params.companyId);
    }

    if (params.branchId) {
      httpParams = httpParams.set('branchId', params.branchId);
    }

    if (params.costCenterId) {
      httpParams = httpParams.set('costCenterId', params.costCenterId);
    }

    if (params.isConsolidated !== undefined) {
      httpParams = httpParams.set('isConsolidated', params.isConsolidated);
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
    companyId?: string;
    branchId?: string;
    costCenterId?: string;
    isConsolidated?: boolean;
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

  preparePayRun(id: string, payload?: PayRunActionRequest): Observable<PayRunDetail> {
    return this.http.post<PayRunDetail>(`${this.baseUrl}/${id}/prepare`, payload ?? {});
  }

  changeStatus(id: string, status: PayRunStatus): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/status`, { status });
  }

  approvePayRun(id: string, payload?: PayRunActionRequest): Observable<PayRunDetail> {
    return this.http.post<PayRunDetail>(`${this.baseUrl}/${id}/approve`, payload ?? {});
  }

  lockPayRun(id: string, payload?: PayRunActionRequest): Observable<PayRunDetail> {
    return this.http.post<PayRunDetail>(`${this.baseUrl}/${id}/lock`, payload ?? {});
  }

  unlockPayRun(id: string, payload?: PayRunActionRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/unlock`, payload ?? {});
  }

  getBankExportTemplates(): Observable<BankExportTemplate[]> {
    return this.http.get<BankExportTemplate[]>(`${this.bankExportUrl}/templates`);
  }

  getPayRunBankExports(payRunId: string): Observable<PayRunBankExport[]> {
    return this.http.get<PayRunBankExport[]>(`${this.baseUrl}/${payRunId}/bank-exports`);
  }

  generateBankExport(
    payRunId: string,
    templateId: string,
    regenerate = false,
  ): Observable<BankExportGenerateResult> {
    const params = regenerate ? new HttpParams().set('regenerate', 'true') : undefined;
    return this.http.post<BankExportGenerateResult>(
      `${this.baseUrl}/${payRunId}/bank-exports`,
      { templateId },
      { params },
    );
  }

  downloadBankExport(exportId: string): Observable<FileExportResult> {
    return this.http.get<FileExportResult>(`${this.bankExportUrl}/${exportId}/download`);
  }

  downloadBankExportErrors(exportId: string): Observable<FileExportResult> {
    return this.http.get<FileExportResult>(`${this.bankExportUrl}/${exportId}/errors`);
  }

  getPaySlip(payRunId: string, paySlipId: string): Observable<PaySlip> {
    return this.http.get<PaySlip>(`${this.baseUrl}/${payRunId}/payslips/${paySlipId}`);
  }

  getApitReport(payRunId: string): Observable<ApitReport> {
    return this.http.get<ApitReport>(`${this.baseUrl}/${payRunId}/apit-report`);
  }

  downloadApitCertificate(payRunId: string, paySlipId: string): Observable<FileExportResult> {
    return this.http.get<FileExportResult>(`${this.baseUrl}/${payRunId}/payslips/${paySlipId}/apit-certificate`);
  }

  getPayslipDocuments(payRunId: string): Observable<PayslipDocument[]> {
    return this.http.get<PayslipDocument[]>(`${this.baseUrl}/${payRunId}/payslips/documents`);
  }

  generatePayslipDocument(
    payRunId: string,
    employeeId: string,
    regenerate = false,
  ): Observable<PayslipDocument> {
    const params = regenerate ? new HttpParams().set('regenerate', 'true') : undefined;
    return this.http.post<PayslipDocument>(
      `${this.baseUrl}/${payRunId}/payslips/${employeeId}/generate`,
      {},
      { params },
    );
  }

  generatePayslipDocumentsBulk(payRunId: string): Observable<PayslipBulkGenerateResult> {
    return this.http.post<PayslipBulkGenerateResult>(`${this.baseUrl}/${payRunId}/payslips/generate-bulk`, {});
  }

  downloadPayslipDocument(documentId: string): Observable<FileExportResult> {
    return this.http.get<FileExportResult>(`${environment.apiBaseUrl}/payslip-documents/${documentId}/download`);
  }
}
