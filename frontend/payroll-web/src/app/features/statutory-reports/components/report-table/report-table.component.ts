import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ReportColumn, ReportRow } from '../../models/report.models';

@Component({
  selector: 'app-report-table',
  templateUrl: './report-table.component.html',
  styleUrls: ['./report-table.component.scss'],
})
export class ReportTableComponent implements OnChanges {
  @Input() columns: ReportColumn[] = [];
  @Input() rows: ReportRow[] = [];
  @Input() groupBy?: string;
  @Input() groupByLabel?: string;
  @Input() loading = false;

  groupedTotals: Record<string, Record<string, number>> = {};
  overallTotals: Record<string, number> = {};

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['rows'] || changes['groupBy'] || changes['columns']) {
      this.calculateTotals();
    }
  }

  isNumericColumn(column: ReportColumn): boolean {
    return column.type === 'currency' || column.type === 'number';
  }

  getColumnClass(column: ReportColumn): Record<string, boolean> {
    return {
      'text-right': this.isNumericColumn(column),
    };
  }

  private calculateTotals(): void {
    this.groupedTotals = {};
    this.overallTotals = {};

    const numericColumns = this.columns.filter(column => this.isNumericColumn(column));

    for (const row of this.rows) {
      for (const column of numericColumns) {
        const value = Number(row[column.field] ?? 0);
        this.overallTotals[column.field] = (this.overallTotals[column.field] || 0) + value;
      }

      if (this.groupBy) {
        const groupKey = String(row[this.groupBy] ?? 'Unknown');
        this.groupedTotals[groupKey] = this.groupedTotals[groupKey] || {};
        for (const column of numericColumns) {
          const value = Number(row[column.field] ?? 0);
          this.groupedTotals[groupKey][column.field] = (this.groupedTotals[groupKey][column.field] || 0) + value;
        }
      }
    }
  }
}
