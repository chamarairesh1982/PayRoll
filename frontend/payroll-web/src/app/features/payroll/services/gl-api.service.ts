import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { FileExportResult } from '../models/apit-report.model';
import {
  GlAccount,
  GlBatchActionRequest,
  GlJournalBatchDetail,
  GlJournalBatchSummary,
  GlMappingEntry,
} from '../models/gl.model';

@Injectable({ providedIn: 'root' })
export class GlApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}/gl`;
  private readonly payRunsUrl = `${environment.apiBaseUrl}/pay-runs`;

  constructor(private http: HttpClient) {}

  getPayRunBatches(payRunId: string): Observable<GlJournalBatchSummary[]> {
    return this.http.get<GlJournalBatchSummary[]>(`${this.payRunsUrl}/${payRunId}/gl/batches`);
  }

  generateBatch(payRunId: string, regenerate = false): Observable<GlJournalBatchDetail> {
    return this.http.post<GlJournalBatchDetail>(`${this.payRunsUrl}/${payRunId}/gl/batches`, null, {
      params: { regenerate: regenerate ? 'true' : 'false' },
    });
  }

  getBatch(batchId: string): Observable<GlJournalBatchDetail> {
    return this.http.get<GlJournalBatchDetail>(`${this.baseUrl}/batches/${batchId}`);
  }

  approveBatch(batchId: string, payload: GlBatchActionRequest): Observable<GlJournalBatchDetail> {
    return this.http.post<GlJournalBatchDetail>(`${this.baseUrl}/batches/${batchId}/approve`, payload);
  }

  exportBatch(batchId: string, format = 'csv'): Observable<FileExportResult> {
    return this.http.get<FileExportResult>(`${this.baseUrl}/batches/${batchId}/export`, { params: { format } });
  }

  getAccounts(): Observable<GlAccount[]> {
    return this.http.get<GlAccount[]>(`${this.baseUrl}/accounts`);
  }

  upsertAccount(account: Partial<GlAccount>): Observable<GlAccount> {
    return this.http.post<GlAccount>(`${this.baseUrl}/accounts`, account);
  }

  deleteAccount(accountId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/accounts/${accountId}`);
  }

  getMappings(): Observable<GlMappingEntry[]> {
    return this.http.get<GlMappingEntry[]>(`${this.baseUrl}/mappings`);
  }

  upsertMapping(mapping: Partial<GlMappingEntry>): Observable<GlMappingEntry> {
    return this.http.post<GlMappingEntry>(`${this.baseUrl}/mappings`, mapping);
  }
}
