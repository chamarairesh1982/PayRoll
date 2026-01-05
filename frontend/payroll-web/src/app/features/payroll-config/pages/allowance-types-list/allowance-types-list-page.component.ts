import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AllowanceType } from '../../models/allowance-type.model';
import { AllowanceTypesApiService } from '../../services/allowance-types-api.service';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-allowance-types-list-page',
  templateUrl: './allowance-types-list-page.component.html',
  styleUrls: ['./allowance-types-list-page.component.scss'],
  providers: [ConfirmationService, MessageService],
})
export class AllowanceTypesListPageComponent implements OnInit {
  columns: DataTableColumn<AllowanceType>[] = [
    { field: 'code', header: 'Registry Code', sortable: true, minWidth: '120px' },
    { field: 'name', header: 'Description', sortable: true, minWidth: '200px' },
    { field: 'basis', header: 'Computation Basis', sortable: true, minWidth: '150px' },
    { field: 'isEpfApplicable', header: 'EPF', type: 'status', sortable: true, minWidth: '100px' },
    { field: 'isEtfApplicable', header: 'ETF', type: 'status', sortable: true, minWidth: '100px' },
    { field: 'isTaxable', header: 'Taxable', type: 'status', sortable: true, minWidth: '100px' },
    { field: 'isActive', header: 'Status', type: 'status', sortable: true, minWidth: '120px' },
  ];

  items: AllowanceType[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  isLoading = false;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';

  constructor(
    private allowanceTypesApi: AllowanceTypesApiService,
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

    this.allowanceTypesApi
      .getAllowanceTypes({ page: this.page, pageSize: this.pageSize, isActive })
      .subscribe({
        next: (result: PaginatedResult<AllowanceType>) => {
          this.items = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Data Retrieval Failure',
            detail: 'Unable to synchronize institutional allowance components.',
          });
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
    this.confirmationService.confirm({
      header: 'Strategic Decommissioning',
      message: `Are you sure you want to decommission '${item.name}'? This action may impact systemic financial rules.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.isLoading = true;
        this.allowanceTypesApi.deleteAllowanceType(item.id).subscribe({
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
              detail: 'Unable to decommission the specified component.',
            });
            this.isLoading = false;
          },
        });
      },
    });
  }
}
