import { Component, ContentChild, EventEmitter, Input, Output, TemplateRef, ViewChild } from '@angular/core';
import { LazyLoadEvent, MenuItem } from 'primeng/api';
import { Table } from 'primeng/table';

export type DataColumnAlign = 'left' | 'center' | 'right';
export type DataColumnType = 'text' | 'date' | 'datetime' | 'number' | 'currency' | 'badge' | 'boolean' | 'status' | 'amount';

export interface DataTableColumn<T> {
  field: keyof T | string;
  header: string;
  type?: DataColumnType;
  align?: DataColumnAlign;
  format?: string;
  sortable?: boolean;
  filterable?: boolean;
  minWidth?: string;
  priority?: number; // For responsive hiding
}

@Component({
  selector: 'app-data-table',
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss'],
})
export class DataTableComponent<T extends Record<string, any>> {
  @Input() columns: DataTableColumn<T>[] = [];
  @Input() data: T[] = [];
  /** @deprecated Use <ng-template #rowActions> content child instead */
  @Input() actionsTemplate?: TemplateRef<T>;
  @Input() loading = false;

  // Legacy/Optional Inputs
  @Input() emptyMessage = 'No records found.';
  @Input() skeletonRows = 5;
  @Input() showColumnFilters = false;
  @Input() globalFilterPlaceholder = 'Search...';
  @Input() first = 0;
  @Input() showGlobalFilter = false;

  // Pagination
  @Input() paginator = true;
  @Input() rows = 20;
  @Input() totalRecords = 0;
  @Input() rowsPerPageOptions = [10, 20, 50, 100];
  @Input() lazy = true;

  // Features
  @Input() selectionMode: 'single' | 'multiple' | null = null;
  @Input() showCurrentPageReport = true;
  @Input() dataKey = 'id';
  @Input() title?: string;

  // Actions
  @Input() globalFilterFields: string[] = [];
  @Input() selection: T | T[] | null = null;

  // Custom Templates
  @ContentChild('rowActions') rowActionsTemplate?: TemplateRef<any>;
  @ContentChild('headerActions') headerActionsTemplate?: TemplateRef<any>;
  @ContentChild('statusTemplate') statusTemplate?: TemplateRef<any>;
  @ContentChild('amountTemplate') amountTemplate?: TemplateRef<any>;

  @Output() selectionChange = new EventEmitter<T | T[] | null>();
  @Output() lazyLoad = new EventEmitter<LazyLoadEvent>();
  @Output() rowSelect = new EventEmitter<T>();
  @Output() rowUnselect = new EventEmitter<T>();

  @ViewChild('dt') table?: Table;

  get hasSelection(): boolean {
    return !!this.selectionMode;
  }



  onLazyLoad(event: LazyLoadEvent): void {
    this.lazyLoad.emit(event);
  }

  onSelectionChange(value: any): void {
    this.selection = value;
    this.selectionChange.emit(value);
  }

  onRowSelect(event: any): void {
    this.rowSelect.emit(event.data);
  }

  onRowUnselect(event: any): void {
    this.rowUnselect.emit(event.data);
  }

  onGlobalFilter(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.table?.filterGlobal(input.value, 'contains');
  }

  clear(): void {
    this.table?.clear();
  }

  getColumnAlign(col: DataTableColumn<T>): string {
    switch (col.align) {
      case 'right': return 'text-right justify-content-end';
      case 'center': return 'text-center justify-content-center';
      default: return 'text-left justify-content-start';
    }
  }

  getColumnClass(col: DataTableColumn<T>): string {
    return this.getColumnAlign(col);
  }
}
