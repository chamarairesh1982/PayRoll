import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { Bank } from '../../models/bank.model';
import { BankBranch } from '../../models/bank-branch.model';
import { BankBranchesApiService } from '../../services/bank-branches-api.service';
import { BanksApiService } from '../../services/banks-api.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-bank-branches-list-page',
  templateUrl: './bank-branches-list-page.component.html',
  styleUrls: ['./bank-branches-list-page.component.scss'],
  providers: [ConfirmationService, MessageService],
})
export class BankBranchesListPageComponent implements OnInit {
  columns: DataTableColumn<BankBranch>[] = [
    { field: 'bankName', header: 'Parent Institution', sortable: true, minWidth: '200px' },
    { field: 'code', header: 'Branch Code', sortable: true, minWidth: '120px' },
    { field: 'name', header: 'Branch Name', sortable: true, minWidth: '200px' },
    { field: 'isActive', header: 'Status', type: 'status', sortable: true, minWidth: '120px' },
  ];

  items: BankBranch[] = [];
  banks: Bank[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';
  filterBankId = '';
  searchTerm = '';

  constructor(
    private bankBranchesApi: BankBranchesApiService,
    private banksApi: BanksApiService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) { }

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

  load(event?: any): void {
    this.isLoading = true;

    if (event) {
      this.page = event.first / event.rows + 1;
      this.pageSize = event.rows;
    }

    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';
    const bankId = this.filterBankId || null;
    const search = this.searchTerm.trim() || null;

    this.bankBranchesApi
      .getBankBranches({ page: this.page, pageSize: this.pageSize, isActive, bankId, search })
      .subscribe({
        next: (result: PaginatedResult<BankBranch>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Data Retrieval Failure',
            detail: 'Unable to synchronize institutional branch infrastructure.',
          });
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
    this.confirmationService.confirm({
      header: 'Infrastructure Decommissioning',
      message: `Are you sure you want to decommission the branch '${item.name}'? This may impact systemic salary disbursement protocols for specific personnel.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.isLoading = true;
        this.bankBranchesApi.deleteBankBranch(item.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Protocol Executed',
              detail: `${item.name} branch has been successfully decommissioned.`,
            });
            this.load();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Execution Failure',
              detail: 'Unable to decommission the specified branch.',
            });
            this.isLoading = false;
          },
        });
      },
    });
  }
}
