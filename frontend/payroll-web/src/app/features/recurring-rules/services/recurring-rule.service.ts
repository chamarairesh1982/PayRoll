import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { RecurringRule, RecurringRuleFormValue, RecurringRuleStatus } from '../models/recurring-rule.model';

@Injectable({ providedIn: 'root' })
export class RecurringRuleService {
  private rules$ = new BehaviorSubject<RecurringRule[]>(this.seedRules());

  getRules(): Observable<RecurringRule[]> {
    return this.rules$.asObservable();
  }

  getRuleById(id: string): Observable<RecurringRule | undefined> {
    const rule = this.rules$.value.find(item => item.id === id);
    return of(rule);
  }

  createRule(payload: RecurringRuleFormValue): Observable<RecurringRule> {
    const rule: RecurringRule = {
      id: this.createId(),
      name: payload.name,
      type: payload.type,
      amountType: payload.amountType,
      amountValue: payload.amountValue,
      frequency: payload.frequency,
      startDate: payload.startDate,
      endDate: payload.endDate ?? null,
      status: payload.status,
      priority: payload.priority,
      scope: {
        type: payload.scopeType,
        companyId: payload.companyId ?? null,
        branchId: payload.branchId ?? null,
        costCenterId: payload.costCenterId ?? null,
        employeeCategory: payload.employeeCategory ?? null,
        employeeIds: payload.employeeIds ?? [],
      },
    };

    this.rules$.next([rule, ...this.rules$.value]);
    return of(rule);
  }

  updateRule(id: string, payload: RecurringRuleFormValue): Observable<RecurringRule | undefined> {
    const updatedRules = this.rules$.value.map(item =>
      item.id === id
        ? {
            ...item,
            name: payload.name,
            type: payload.type,
            amountType: payload.amountType,
            amountValue: payload.amountValue,
            frequency: payload.frequency,
            startDate: payload.startDate,
            endDate: payload.endDate ?? null,
            status: payload.status,
            priority: payload.priority,
            scope: {
              type: payload.scopeType,
              companyId: payload.companyId ?? null,
              branchId: payload.branchId ?? null,
              costCenterId: payload.costCenterId ?? null,
              employeeCategory: payload.employeeCategory ?? null,
              employeeIds: payload.employeeIds ?? [],
            },
          }
        : item,
    );
    this.rules$.next(updatedRules);
    return this.getRuleById(id);
  }

  deleteRule(id: string): Observable<boolean> {
    this.rules$.next(this.rules$.value.filter(item => item.id !== id));
    return of(true);
  }

  setStatus(id: string, status: RecurringRuleStatus): Observable<boolean> {
    this.rules$.next(
      this.rules$.value.map(item => (item.id === id ? { ...item, status } : item)),
    );
    return of(true);
  }

  bulkUpdateStatus(ids: string[], status: RecurringRuleStatus): Observable<boolean> {
    const idSet = new Set(ids);
    this.rules$.next(this.rules$.value.map(item => (idSet.has(item.id) ? { ...item, status } : item)));
    return of(true);
  }

  private createId(): string {
    return `rr_${Math.random().toString(36).slice(2, 10)}`;
  }

  private seedRules(): RecurringRule[] {
    return [
      {
        id: 'rr_loan_001',
        name: 'Employee Loan Installment',
        type: 'Loan installment',
        amountType: 'Fixed',
        amountValue: 8500,
        frequency: 'Monthly',
        startDate: '2024-01-01',
        endDate: null,
        status: 'Active',
        priority: 1,
        scope: {
          type: 'Group',
          companyId: 'COMP-001',
          branchId: null,
          costCenterId: null,
          employeeCategory: 'Permanent',
          employeeIds: [],
        },
      },
      {
        id: 'rr_allow_002',
        name: 'Fuel Allowance',
        type: 'Allowance',
        amountType: 'Fixed',
        amountValue: 12000,
        frequency: 'Monthly',
        startDate: '2024-02-01',
        endDate: null,
        status: 'Active',
        priority: 2,
        scope: {
          type: 'All',
          companyId: null,
          branchId: null,
          costCenterId: null,
          employeeCategory: null,
          employeeIds: [],
        },
      },
      {
        id: 'rr_ded_003',
        name: 'Health Contribution',
        type: 'Deduction',
        amountType: 'Percentage',
        amountValue: 2.5,
        frequency: 'Monthly',
        startDate: '2024-01-15',
        endDate: null,
        status: 'Inactive',
        priority: 3,
        scope: {
          type: 'Selected',
          companyId: null,
          branchId: null,
          costCenterId: null,
          employeeCategory: null,
          employeeIds: ['EMP-001', 'EMP-003'],
        },
      },
    ];
  }
}
