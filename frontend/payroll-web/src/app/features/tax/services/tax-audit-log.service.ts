import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { TaxAuditLogEntry } from '../models/tax-audit-log.model';
import { TaxScheme } from '../models/tax-scheme.model';

@Injectable({ providedIn: 'root' })
export class TaxAuditLogService {
  private logsSubject = new BehaviorSubject<TaxAuditLogEntry[]>(this.seedLogs());

  getLogs(): Observable<TaxAuditLogEntry[]> {
    return this.logsSubject.asObservable();
  }

  appendLog(entry: Omit<TaxAuditLogEntry, 'id' | 'changedAt'>): void {
    const log: TaxAuditLogEntry = {
      ...entry,
      id: `tax_log_${Math.random().toString(36).slice(2, 10)}`,
      changedAt: new Date().toISOString(),
    };
    this.logsSubject.next([log, ...this.logsSubject.value]);
  }

  private seedLogs(): TaxAuditLogEntry[] {
    const now = new Date();
    const baseScheme: Partial<TaxScheme> = {
      name: '2024/2025 PAYE Scheme',
      effectiveFrom: '2024-04-01',
    };

    return [
      {
        id: 'tax_log_seed_01',
        schemeId: 'tax_scheme_2024',
        schemeName: '2024/2025 PAYE Scheme',
        action: 'Created',
        changedAt: now.toISOString(),
        changedBy: 'System',
        summary: 'Initial scheme seeded for preview use.',
        before: null,
        after: baseScheme,
      },
    ];
  }
}
