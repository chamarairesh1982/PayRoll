import { Component, EventEmitter, Input, Output, TemplateRef, ViewChild } from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';

type DataColumnType = 'text' | 'date' | 'datetime' | 'number' | 'currency';

export interface DataTableColumn<T> {
  field: keyof T | string;
  header: string;
  type?: DataColumnType;
  format?: string;
  filter?: boolean;
  filterMatchMode?: string;
  sortable?: boolean;
}

@Component({
  selector: 'app-data-table',
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss'],
})
export class DataTableComponent<T extends Record<string, unknown>> {
  @Input() columns: DataTableColumn<T>[] = [];
  @Input() data: T[] = [];
  @Input() actionsTemplate?: TemplateRef<T>;
  @Input() loading = false;
  @Input('totalRecords') totalRecords = 0;
  @Input('rows') rows = 25;
  @Input() first = 0;
  @Input() rowsPerPageOptions: number[] = [10, 25, 50];
  @Input() paginator = false;
  @Input() lazy = false;
  @Input() showGlobalFilter = false;
  @Input() showColumnFilters = false;
  @Input() globalFilterFields?: string[];
  @Input() globalFilterPlaceholder = 'Search';
  @Input() emptyMessage = 'No records found.';
  @Input() skeletonRows = 6;

  @Output() lazyLoad = new EventEmitter<LazyLoadEvent>();

  @ViewChild('dt') table?: Table;

  get resolvedGlobalFilterFields(): string[] {
    return this.globalFilterFields ?? this.columns.map(column => column.field.toString());
  }

  get skeletonRowsArray(): number[] {
    return Array.from({ length: this.skeletonRows });
  }

  onGlobalFilter(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.table?.filterGlobal(target.value, 'contains');
  }

  handleLazyLoad(event: LazyLoadEvent): void {
    this.lazyLoad.emit(event);
  }

  resolveFilterType(column: DataTableColumn<T>): string {
    switch (column.type) {
      case 'date':
      case 'datetime':
        return 'date';
      case 'number':
      case 'currency':
        return 'numeric';
      default:
        return 'text';
    }
  }
}
