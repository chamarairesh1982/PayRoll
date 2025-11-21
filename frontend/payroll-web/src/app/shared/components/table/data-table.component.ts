import { Component, Input, TemplateRef } from '@angular/core';

@Component({
  selector: 'app-data-table',
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss'],
})
export class DataTableComponent<T extends Record<string, unknown>> {
  @Input() columns: { field: keyof T; header: string }[] = [];
  @Input() data: T[] = [];
  @Input() actionsTemplate?: TemplateRef<T>;
}
