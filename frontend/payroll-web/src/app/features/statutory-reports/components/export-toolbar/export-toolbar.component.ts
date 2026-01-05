import { Component, Input } from '@angular/core';
import { ReportColumn, ReportRow, ReportKey } from '../../models/report.models';

@Component({
  selector: 'app-export-toolbar',
  templateUrl: './export-toolbar.component.html',
  styleUrls: ['./export-toolbar.component.scss'],
})
export class ExportToolbarComponent {
  @Input() reportKey: ReportKey | null = null;
  @Input() reportTitle = '';
  @Input() columns: ReportColumn[] = [];
  @Input() rows: ReportRow[] = [];
  @Input() excelSupported = false;
  @Input() pdfSupported = false;

  export(format: 'csv' | 'excel' | 'pdf'): void {
    if (format === 'csv') {
      this.exportCsv();
    }
  }

  exportCsv(): void {
    const header = this.columns.map(column => this.escapeCsv(column.header)).join(',');
    const body = this.rows
      .map(row =>
        this.columns
          .map(column => this.escapeCsv(String(row[column.field] ?? '')))
          .join(','),
      )
      .join('\n');

    const csvContent = [header, body].filter(Boolean).join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `${this.reportTitle || 'statutory-report'}.csv`;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  private escapeCsv(value: string): string {
    if (value.includes(',') || value.includes('"') || value.includes('\n')) {
      return `"${value.replace(/"/g, '""')}"`;
    }
    return value;
  }
}
