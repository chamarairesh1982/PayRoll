import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { Bank } from '../../models/bank.model';
import { BankBranch } from '../../models/bank-branch.model';
import { BankBranchesApiService } from '../../services/bank-branches-api.service';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-bank-branches-list-page',
  templateUrl: './bank-branches-list-page.component.html',
  styleUrls: ['./bank-branches-list-page.component.scss'],
})
export class BankBranchesListPageComponent implements OnInit {
  items: BankBranch[] = [];
  banks: Bank[] = [];
  page = 1;
  pageSize = 50;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';
  filterBankId = '';
  searchTerm = '';

  showConfirm = false;
  selectedItem: BankBranch | null = null;

  constructor(
    private bankBranchesApi: BankBranchesApiService,
    private banksApi: BanksApiService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadBanks();
    this.load();
  }

  loadBanks(): void {
    this.banksApi.getBanks({ page: 1, pageSize: 200, isActive: null }).subscribe({
      next: result => {
        this.banks = result.items;
      },
      error: err => console.error('Failed to load banks', err),
    });
  }

  load(): void {
    this.isLoading = true;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';
    const bankId = this.filterBankId || null;
    const search = this.searchTerm.trim() || null;

    this.bankBranchesApi
      .getBankBranches({ page: this.page, pageSize: this.pageSize, isActive, bankId, search })
      .subscribe({
        next: (result: PaginatedResult<BankBranch>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load bank branches', err);
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
    this.router.navigate(['/config/bank-branches/new']);
  }

  navigateToView(item: BankBranch): void {
    this.router.navigate(['/config/bank-branches', item.id]);
  }

  navigateToEdit(item: BankBranch): void {
    this.router.navigate(['/config/bank-branches', item.id, 'edit']);
  }

  confirmDelete(item: BankBranch): void {
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

    this.bankBranchesApi.deleteBankBranch(this.selectedItem.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.load();
      },
      error: err => {
        console.error('Failed to delete bank branch', err);
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
