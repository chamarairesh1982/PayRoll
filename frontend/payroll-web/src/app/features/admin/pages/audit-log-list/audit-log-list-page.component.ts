import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { AuditLogEntry } from '../../../../shared/models/audit-log.model';
import { AuditLogApiService } from '../../../../shared/services/audit-log-api.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

type AuditLogRow = AuditLogEntry & { beforePreview: string; afterPreview: string; createdAtDisplay: string };

@Component({
  selector: 'app-audit-log-list-page',
  templateUrl: './audit-log-list-page.component.html',
  styleUrls: ['./audit-log-list-page.component.scss'],
  providers: [MessageService],
})
export class AuditLogListPageComponent implements OnInit {
  columns: DataTableColumn<AuditLogRow>[] = [
    { field: 'createdAtDisplay', header: 'Chronology', sortable: true, minWidth: '180px' },
    { field: 'actorDisplayName', header: 'Principal Actor', sortable: true, minWidth: '180px' },
    { field: 'entityType', header: 'Governance Object', sortable: true, minWidth: '150px' },
    { field: 'action', header: 'Transaction Type', sortable: true, minWidth: '150px' },
    { field: 'afterPreview', header: 'Post-State Intelligence', sortable: false, minWidth: '300px' },
  ];

  logs: AuditLogRow[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  isLoading = false;

  filters: {
    entityType: string;
    entityId: string;
    action: string;
    actor: string;
    from: string;
    to: string;
  } = {
      entityType: '',
      entityId: '',
      action: '',
      actor: '',
      from: '',
      to: '',
    };

  constructor(
    private auditApi: AuditLogApiService,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.load();
  }

  load(event?: any): void {
    this.isLoading = true;

    if (event) {
      this.page = event.first / event.rows + 1;
      this.pageSize = event.rows;
    }

    this.auditApi
      .getAuditLogs({
        page: this.page,
        pageSize: this.pageSize,
        entityType: this.filters.entityType || undefined,
        entityId: this.filters.entityId || undefined,
        action: this.filters.action || undefined,
        actor: this.filters.actor || undefined,
        from: this.filters.from || undefined,
        to: this.filters.to || undefined,
      })
      .subscribe({
        next: (result: PaginatedResult<AuditLogEntry>) => {
          this.logs = result.items.map(log => this.toRow(log));
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Data Retrieval Failure',
            detail: 'Unable to synchronize institutional audit trails.',
          });
          this.isLoading = false;
        },
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  clearFilters(): void {
    this.filters = {
      entityType: '',
      entityId: '',
      action: '',
      actor: '',
      from: '',
      to: '',
    };
    this.applyFilters();
  }

  export(): void {
    this.auditApi
      .exportAuditLogs({
        entityType: this.filters.entityType || undefined,
        entityId: this.filters.entityId || undefined,
        action: this.filters.action || undefined,
        actor: this.filters.actor || undefined,
        from: this.filters.from || undefined,
        to: this.filters.to || undefined,
      })
      .subscribe({
        next: blob => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `audit-logs-${new Date().toISOString()}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.messageService.add({
            severity: 'success',
            summary: 'Export Successful',
            detail: 'Institutional audit trail has been archived to CSV.',
          });
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Export Failure',
            detail: 'Unable to archive the specified audit trail segment.',
          });
        },
      });
  }

  private toRow(log: AuditLogEntry): AuditLogRow {
    return {
      ...log,
      actorDisplayName: log.actorDisplayName || log.actorUserId,
      beforePreview: this.preview(log.beforeJson ?? ''),
      afterPreview: this.preview(log.afterJson ?? ''),
      createdAtDisplay: new Date(log.timestampUtc).toLocaleString(),
    };
  }

  private preview(value: string): string {
    if (!value) {
      return '';
    }

    const limit = 120;
    return value.length > limit ? `${value.slice(0, limit)}...` : value;
  }
}
