import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-table-toolbar',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
  templateUrl: './table-toolbar.component.html',
  styleUrls: ['./table-toolbar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableToolbarComponent {
  @Input() searchPlaceholder = 'Search';
  @Input() filterLabel = 'Filters';
  @Input() columnsLabel = 'Columns';
  @Input() viewsLabel = 'Saved views';
  @Input() exportLabel = 'Export';

  @Output() searchChange = new EventEmitter<string>();
  @Output() toggleFilters = new EventEmitter<void>();
  @Output() openColumnChooser = new EventEmitter<void>();
  @Output() openSavedViews = new EventEmitter<void>();
  @Output() exportData = new EventEmitter<void>();

  searchTerm = '';

  onSearchChange(value: string): void {
    this.searchChange.emit(value);
  }
}
