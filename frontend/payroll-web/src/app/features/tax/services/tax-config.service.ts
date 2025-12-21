import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { TaxAuditLogService } from './tax-audit-log.service';
import { TaxRelief, TaxScheme } from '../models/tax-scheme.model';

@Injectable({ providedIn: 'root' })
export class TaxConfigService {
  private schemesSubject = new BehaviorSubject<TaxScheme[]>(this.seedSchemes());

  constructor(private auditLogService: TaxAuditLogService) {}

  getSchemes(): Observable<TaxScheme[]> {
    return this.schemesSubject.asObservable();
  }

  getActiveSchemeForDate(date: Date): Observable<TaxScheme | null> {
    const target = date.toISOString().split('T')[0];
    const scheme = this.schemesSubject.value.find(item => {
      if (!item.isActive) {
        return false;
      }
      const start = item.effectiveFrom;
      const end = item.effectiveTo ?? '9999-12-31';
      return target >= start && target <= end;
    });
    return of(scheme ?? null);
  }

  createScheme(payload: TaxScheme): void {
    this.schemesSubject.next([payload, ...this.schemesSubject.value]);
    this.auditLogService.appendLog({
      schemeId: payload.id,
      schemeName: payload.name,
      action: 'Created',
      changedBy: 'Admin',
      summary: `Created ${payload.name}.`,
      before: null,
      after: payload,
    });
  }

  updateScheme(payload: TaxScheme): void {
    const previous = this.schemesSubject.value.find(item => item.id === payload.id) ?? null;
    this.schemesSubject.next(this.schemesSubject.value.map(item => (item.id === payload.id ? payload : item)));
    this.auditLogService.appendLog({
      schemeId: payload.id,
      schemeName: payload.name,
      action: 'Updated',
      changedBy: 'Admin',
      summary: `Updated ${payload.name}.`,
      before: previous,
      after: payload,
    });
  }

  cloneScheme(payload: TaxScheme, clonedFrom: TaxScheme): void {
    this.schemesSubject.next([payload, ...this.schemesSubject.value]);
    this.auditLogService.appendLog({
      schemeId: payload.id,
      schemeName: payload.name,
      action: 'Cloned',
      changedBy: 'Admin',
      summary: `Cloned ${clonedFrom.name} into ${payload.name}.`,
      before: clonedFrom,
      after: payload,
    });
  }

  createRelief(): TaxRelief {
    return {
      id: `relief_${Math.random().toString(36).slice(2, 10)}`,
      description: 'New relief',
      amount: 0,
      reliefType: 'IncomeRelief',
      frequency: 'Monthly',
    };
  }

  createSchemeId(): string {
    return `tax_scheme_${Math.random().toString(36).slice(2, 10)}`;
  }

  private seedSchemes(): TaxScheme[] {
    return [
      {
        id: 'tax_scheme_2024',
        name: '2024/2025 PAYE Scheme',
        effectiveFrom: '2024-04-01',
        effectiveTo: '2025-03-31',
        isActive: true,
        slabs: [
          { from: 0, to: 100000, rate: 0 },
          { from: 100000, to: 200000, rate: 6 },
          { from: 200000, to: 350000, rate: 12 },
          { from: 350000, to: 450000, rate: 18 },
          { from: 450000, to: 600000, rate: 24 },
          { from: 600000, to: null, rate: 30 },
        ],
        reliefs: [
          {
            id: 'relief_001',
            description: 'Monthly personal relief',
            amount: 10000,
            reliefType: 'IncomeRelief',
            frequency: 'Monthly',
          },
        ],
      },
    ];
  }
}
