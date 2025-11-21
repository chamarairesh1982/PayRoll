import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { EpfEtfRuleSet } from '../../models/epf-etf-rule-set.model';
import { EpfEtfRulesApiService } from '../../services/epf-etf-rules-api.service';

@Component({
  selector: 'app-epf-etf-rules-list-page',
  templateUrl: './epf-etf-rules-list-page.component.html',
  styleUrls: ['./epf-etf-rules-list-page.component.scss'],
})
export class EpfEtfRulesListPageComponent implements OnInit {
  items: EpfEtfRuleSet[] = [];
  page = 1;
  pageSize = 20;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';

  constructor(private epfEtfApi: EpfEtfRulesApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';

    this.epfEtfApi
      .getRuleSets({ page: this.page, pageSize: this.pageSize, isActive })
      .subscribe({
        next: (result: PaginatedResult<EpfEtfRuleSet>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load EPF/ETF rules', err);
          this.isLoading = false;
        },
      });
  }

  handleFilterChange(): void {
    this.page = 1;
    this.load();
  }

  navigateToCreate(): void {
    this.router.navigate(['/config/epf-etf/new']);
  }

  navigateToEdit(item: EpfEtfRuleSet): void {
    this.router.navigate(['/config/epf-etf', item.id, 'edit']);
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

  get totalPages(): number {
    return this.pageSize ? Math.ceil(this.totalCount / this.pageSize) : 1;
  }
}
