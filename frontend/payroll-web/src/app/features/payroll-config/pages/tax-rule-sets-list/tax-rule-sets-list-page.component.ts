import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { TaxRuleSet } from '../../models/tax-rule-set.model';
import { TaxRuleSetsApiService } from '../../services/tax-rule-sets-api.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-tax-rule-sets-list-page',
  templateUrl: './tax-rule-sets-list-page.component.html',
  styleUrls: ['./tax-rule-sets-list-page.component.scss'],
  providers: [MessageService],
})
export class TaxRuleSetsListPageComponent implements OnInit {
  columns: DataTableColumn<TaxRuleSet>[] = [
    { field: 'name', header: 'Framework Name', sortable: true, minWidth: '200px' },
    { field: 'yearOfAssessment', header: 'Fiscal Year', sortable: true, minWidth: '120px' },
    { field: 'effectiveFrom', header: 'Compliance Start', type: 'date', sortable: true, minWidth: '150px' },
    { field: 'effectiveTo', header: 'Compliance End', type: 'date', sortable: true, minWidth: '150px' },
    { field: 'isDefault', header: 'System Default', type: 'status', sortable: true, minWidth: '130px' },
    { field: 'isActive', header: 'Status', type: 'status', sortable: true, minWidth: '120px' },
  ];

  items: TaxRuleSet[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  isLoading = false;
  filterYear: number | null = null;

  constructor(
    private taxApi: TaxRuleSetsApiService,
    private router: Router,
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

    this.taxApi
      .getRuleSets({ page: this.page, pageSize: this.pageSize, yearOfAssessment: this.filterYear })
      .subscribe({
        next: (result: PaginatedResult<TaxRuleSet>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Data Retrieval Failure',
            detail: 'Unable to synchronize institutional tax governance frameworks.',
          });
          this.isLoading = false;
        },
      });
  }

  handleFilterChange(): void {
    this.page = 1;
    this.load();
  }

  handleYearChange(year: string | number | null): void {
    if (year === null || year === undefined || year === '') {
      this.filterYear = null;
    } else {
      this.filterYear = Number(year);
    }
    this.handleFilterChange();
  }

  navigateToCreate(): void {
    this.router.navigate(['/config/tax-rules/new']);
  }

  navigateToEdit(item: TaxRuleSet): void {
    this.router.navigate(['/config/tax-rules', item.id, 'edit']);
  }

  clearYear(): void {
    this.filterYear = null;
    this.handleFilterChange();
  }
}
