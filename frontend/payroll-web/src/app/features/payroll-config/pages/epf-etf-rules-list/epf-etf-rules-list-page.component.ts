import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LazyLoadEvent, MessageService } from 'primeng/api';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';
import { EpfEtfRuleSet } from '../../models/epf-etf-rule-set.model';
import { EpfEtfRulesApiService } from '../../services/epf-etf-rules-api.service';

@Component({
  selector: 'app-epf-etf-rules-list-page',
  templateUrl: './epf-etf-rules-list-page.component.html',
  styleUrls: ['./epf-etf-rules-list-page.component.scss'],
  providers: [MessageService]
})
export class EpfEtfRulesListPageComponent implements OnInit {
  items: EpfEtfRuleSet[] = [];
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';

  columns: DataTableColumn<EpfEtfRuleSet>[] = [
    { field: 'name', header: 'Governance Policy', sortable: true },
    { field: 'effectiveFrom', header: 'Enforcement Start', type: 'date', sortable: true },
    { field: 'effectiveTo', header: 'Enforcement End', type: 'date', sortable: true },
    { field: 'employeeEpfRate', header: 'EE EPF %', sortable: true },
    { field: 'employerEpfRate', header: 'ER EPF %', sortable: true },
    { field: 'employerEtfRate', header: 'ER ETF %', sortable: true },
    { field: 'isDefault', header: 'Primary', type: 'boolean' },
    { field: 'isActive', header: 'Status', type: 'boolean' }
  ];

  constructor(
    private epfEtfApi: EpfEtfRulesApiService,
    private router: Router,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    // Initial load will be triggered by app-data-table's onLazyLoad
  }

  load(event?: LazyLoadEvent): void {
    this.isLoading = true;
    const page = event ? Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1 : 1;
    const pageSize = event?.rows ?? 10;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';

    this.epfEtfApi
      .getRuleSets({ page, pageSize, isActive })
      .subscribe({
        next: result => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Systemic Failure',
            detail: 'Failed to synchronize EPF/ETF governance protocols.'
          });
          this.isLoading = false;
        },
      });
  }

  handleFilterChange(): void {
    this.load();
  }

  navigateToCreate(): void {
    this.router.navigate(['/config/epf-etf/new']);
  }

  navigateToEdit(item: EpfEtfRuleSet): void {
    this.router.navigate(['/config/epf-etf', item.id, 'edit']);
  }
}
