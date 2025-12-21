import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { TaxAuditLogEntry } from '../../models/tax-audit-log.model';
import { TaxAuditLogService } from '../../services/tax-audit-log.service';

@Component({
  selector: 'app-tax-audit-log-page',
  templateUrl: './tax-audit-log-page.component.html',
  styleUrls: ['./tax-audit-log-page.component.scss'],
})
export class TaxAuditLogPageComponent implements OnInit, OnDestroy {
  logs: TaxAuditLogEntry[] = [];

  private destroy$ = new Subject<void>();

  constructor(private auditLogService: TaxAuditLogService) {}

  ngOnInit(): void {
    this.auditLogService
      .getLogs()
      .pipe(takeUntil(this.destroy$))
      .subscribe(logs => (this.logs = logs));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
