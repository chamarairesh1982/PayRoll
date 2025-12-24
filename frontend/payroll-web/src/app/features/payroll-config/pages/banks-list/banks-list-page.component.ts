import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { Bank } from '../../models/bank.model';
import { BanksApiService } from '../../services/banks-api.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-banks-list-page',
  templateUrl: './banks-list-page.component.html',
  styleUrls: ['./banks-list-page.component.scss'],
  providers: [ConfirmationService, MessageService],
})
export class BanksListPageComponent implements OnInit {
  columns: DataTableColumn<Bank>[] = [
    { field: 'code', header: 'SWIFT / Local Code', sortable: true, minWidth: '150px' },
    { field: 'name', header: 'Institution Name', sortable: true, minWidth: '250px' },
    { field: 'isActive', header: 'Operational Status', type: 'status', sortable: true, minWidth: '150px' },
  ];

  items: Bank[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';
  searchTerm = '';

  constructor(
    private banksApi: BanksApiService,
    private router: Router,
    private confirmationService: ConfirmationService,
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

    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';
    const search = this.searchTerm.trim() || null;

    this.banksApi
      .getBanks({ page: this.page, pageSize: this.pageSize, isActive, search })
      .subscribe({
        next: (result: PaginatedResult<Bank>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Data Retrieval Failure',
            detail: 'Unable to synchronize institutional banking infrastructure.',
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
    this.router.navigate(['/config/banks/new']);
  }

  navigateToView(item: Bank): void {
    this.router.navigate(['/config/banks', item.id]);
  }

  navigateToEdit(item: Bank): void {
    this.router.navigate(['/config/banks', item.id, 'edit']);
  }

  confirmDelete(item: Bank): void {
    this.confirmationService.confirm({
      header: 'Infrastructure Decommissioning',
      message: `Are you sure you want to decommission the banking institution '${item.name}'? This may impact systemic salary disbursement protocols.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.isLoading = true;
        this.banksApi.deleteBank(item.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Protocol Executed',
              detail: `${item.name} has been successfully decommissioned.`,
            });
            this.load();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Execution Failure',
              detail: 'Unable to decommission the specified institution.',
            });
            this.isLoading = false;
          },
        });
      },
    });
  }
}
