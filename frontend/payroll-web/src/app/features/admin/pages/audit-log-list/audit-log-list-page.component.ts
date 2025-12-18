import { Component, OnInit } from '@angular/core';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { AuditLogEntry } from '../../../../shared/models/audit-log.model';
import { AuditLogApiService } from '../../../../shared/services/audit-log-api.service';

type AuditLogRow = AuditLogEntry & { beforePreview: string; afterPreview: string; createdAtDisplay: string };

@Component({
  selector: 'app-audit-log-list-page',
  templateUrl: './audit-log-list-page.component.html',
  styleUrls: ['./audit-log-list-page.component.scss'],
})
export class AuditLogListPageComponent implements OnInit {
  logs: AuditLogRow[] = [];
  page = 1;
  pageSize = 25;
  totalCount = 0;
  isLoading = false;

  filters: {
    entityName: string;
    entityId: string;
    action: string;
    createdBy: string;
    from: string;
    to: string;
  } = {
    entityName: '',
    entityId: '',
    action: '',
    createdBy: '',
    from: '',
    to: '',
  };

  columns: { field: keyof AuditLogRow; header: string }[] = [
    { field: 'createdAtDisplay', header: 'Timestamp' },
    { field: 'createdBy', header: 'Actor' },
    { field: 'entityName', header: 'Entity' },
    { field: 'entityId', header: 'Entity ID' },
    { field: 'action', header: 'Action' },
    { field: 'beforePreview', header: 'Before' },
    { field: 'afterPreview', header: 'After' },
  ];

  constructor(private auditApi: AuditLogApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.auditApi
      .getAuditLogs({
        page: this.page,
        pageSize: this.pageSize,
        entityName: this.filters.entityName || undefined,
        entityId: this.filters.entityId || undefined,
        action: this.filters.action || undefined,
        createdBy: this.filters.createdBy || undefined,
        from: this.filters.from || undefined,
        to: this.filters.to || undefined,
      })
      .subscribe({
        next: (result: PaginatedResult<AuditLogEntry>) => {
          this.logs = result.items.map(log => this.toRow(log));
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load audit logs', err);
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
      entityName: '',
      entityId: '',
      action: '',
      createdBy: '',
      from: '',
      to: '',
    };
    this.applyFilters();
  }

  export(): void {
    this.auditApi
      .exportAuditLogs({
        entityName: this.filters.entityName || undefined,
        entityId: this.filters.entityId || undefined,
        action: this.filters.action || undefined,
        createdBy: this.filters.createdBy || undefined,
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
        },
        error: err => console.error('Failed to export audit logs', err),
      });
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.load();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }

  private toRow(log: AuditLogEntry): AuditLogRow {
    return {
      ...log,
      beforePreview: this.preview(log.beforeSnapshot),
      afterPreview: this.preview(log.afterSnapshot),
      createdAtDisplay: new Date(log.createdAt).toLocaleString(),
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
