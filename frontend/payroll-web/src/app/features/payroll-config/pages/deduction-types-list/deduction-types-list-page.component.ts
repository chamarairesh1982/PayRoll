import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { DeductionType } from '../../models/deduction-type.model';
import { DeductionTypesApiService } from '../../services/deduction-types-api.service';

@Component({
  selector: 'app-deduction-types-list-page',
  templateUrl: './deduction-types-list-page.component.html',
  styleUrls: ['./deduction-types-list-page.component.scss'],
})
export class DeductionTypesListPageComponent implements OnInit {
  items: DeductionType[] = [];
  page = 1;
  pageSize = 50;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';

  showConfirm = false;
  selectedItem: DeductionType | null = null;

  constructor(private deductionTypesApi: DeductionTypesApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';

    this.deductionTypesApi
      .getDeductionTypes({ page: this.page, pageSize: this.pageSize, isActive })
      .subscribe({
        next: (result: PaginatedResult<DeductionType>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load deduction types', err);
          this.isLoading = false;
        },
      });
  }

  handleFilterChange(): void {
    this.page = 1;
    this.load();
  }

  navigateToCreate(): void {
    this.router.navigate(['/config/deductions/new']);
  }

  navigateToEdit(item: DeductionType): void {
    this.router.navigate(['/config/deductions', item.id, 'edit']);
  }

  confirmDelete(item: DeductionType): void {
    this.selectedItem = item;
    this.showConfirm = true;
  }

  cancelDelete(): void {
    this.selectedItem = null;
    this.showConfirm = false;
  }

  deleteItem(): void {
    if (!this.selectedItem) {
      return;
    }

    this.deductionTypesApi.deleteDeductionType(this.selectedItem.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.load();
      },
      error: err => {
        console.error('Failed to delete deduction type', err);
        this.cancelDelete();
      },
    });
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
