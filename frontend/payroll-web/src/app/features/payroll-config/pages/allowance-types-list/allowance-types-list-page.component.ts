import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AllowanceType } from '../../models/allowance-type.model';
import { AllowanceTypesApiService } from '../../services/allowance-types-api.service';
import { PaginatedResult } from '../../../employees/models/employee.model';

@Component({
  selector: 'app-allowance-types-list-page',
  templateUrl: './allowance-types-list-page.component.html',
  styleUrls: ['./allowance-types-list-page.component.scss'],
})
export class AllowanceTypesListPageComponent implements OnInit {
  items: AllowanceType[] = [];
  page = 1;
  pageSize = 50;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';

  showConfirm = false;
  selectedItem: AllowanceType | null = null;

  constructor(private allowanceTypesApi: AllowanceTypesApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';

    this.allowanceTypesApi
      .getAllowanceTypes({ page: this.page, pageSize: this.pageSize, isActive })
      .subscribe({
        next: (result: PaginatedResult<AllowanceType>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load allowance types', err);
          this.isLoading = false;
        },
      });
  }

  handleFilterChange(): void {
    this.page = 1;
    this.load();
  }

  navigateToCreate(): void {
    this.router.navigate(['/config/allowances/new']);
  }

  navigateToEdit(item: AllowanceType): void {
    this.router.navigate(['/config/allowances', item.id, 'edit']);
  }

  confirmDelete(item: AllowanceType): void {
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

    this.allowanceTypesApi.deleteAllowanceType(this.selectedItem.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.load();
      },
      error: err => {
        console.error('Failed to delete allowance type', err);
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
