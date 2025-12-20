import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { Bank } from '../../models/bank.model';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-banks-list-page',
  templateUrl: './banks-list-page.component.html',
  styleUrls: ['./banks-list-page.component.scss'],
})
export class BanksListPageComponent implements OnInit {
  items: Bank[] = [];
  page = 1;
  pageSize = 50;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';
  searchTerm = '';

  showConfirm = false;
  selectedItem: Bank | null = null;

  constructor(private banksApi: BanksApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';
    const search = this.searchTerm.trim() || null;

    this.banksApi
      .getBanks({ page: this.page, pageSize: this.pageSize, isActive, search })
      .subscribe({
        next: (result: PaginatedResult<Bank>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load banks', err);
          this.isLoading = false;
        },
      });
  }

  handleFilterChange(): void {
    this.page = 1;
    this.load();
  }

  handleSearch(): void {
    this.page = 1;
    this.load();
  }

  navigateToCreate(): void {
    this.router.navigate(['/config/banks/new']);
  }

  navigateToView(item: Bank): void {
    this.router.navigate(['/config/banks', item.id]);
  }

  navigateToEdit(item: Bank): void {
    this.router.navigate(['/config/banks', item.id, 'edit']);
  }

  confirmDelete(item: Bank): void {
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

    this.banksApi.deleteBank(this.selectedItem.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.load();
      },
      error: err => {
        console.error('Failed to delete bank', err);
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
