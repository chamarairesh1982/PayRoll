import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { TaxRuleSet } from '../../models/tax-rule-set.model';
import { TaxRuleSetsApiService } from '../../services/tax-rule-sets-api.service';

@Component({
  selector: 'app-tax-rule-sets-list-page',
  templateUrl: './tax-rule-sets-list-page.component.html',
  styleUrls: ['./tax-rule-sets-list-page.component.scss'],
})
export class TaxRuleSetsListPageComponent implements OnInit {
  items: TaxRuleSet[] = [];
  page = 1;
  pageSize = 20;
  totalCount = 0;
  isLoading = false;
  filterYear: number | null = null;

  constructor(private taxApi: TaxRuleSetsApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.taxApi
      .getRuleSets({ page: this.page, pageSize: this.pageSize, yearOfAssessment: this.filterYear })
      .subscribe({
        next: (result: PaginatedResult<TaxRuleSet>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load tax rule sets', err);
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

  clearYear(): void {
    this.filterYear = null;
    this.handleFilterChange();
  }

  get totalPages(): number {
    return this.pageSize ? Math.ceil(this.totalCount / this.pageSize) : 1;
  }
}
