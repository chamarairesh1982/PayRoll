import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PaginatorState } from 'primeng/paginator';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/services/auth.service';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee, PaginatedResult } from '../../models/employee.model';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

type EmployeeRow = Employee & { nicDisplay: string };

@Component({
  selector: 'app-employees-list-page',
  templateUrl: './employees-list-page.component.html',
  styleUrls: ['./employees-list-page.component.scss'],
  providers: [ConfirmationService, MessageService],
})
export class EmployeesListPageComponent implements OnInit, OnDestroy {
  employees: EmployeeRow[] = [];
  loading = false;
  totalCount = 0;
  page = 1;
  pageSize = 25;

  // Power Grid Columns
  columns: DataTableColumn<EmployeeRow>[] = [
    { field: 'employeeCode', header: 'Code', sortable: true, filterable: true, minWidth: '100px' },
    { field: 'firstName', header: 'First Name', sortable: true, filterable: true, minWidth: '120px' },
    { field: 'lastName', header: 'Last Name', sortable: true, filterable: true, minWidth: '120px' },
    { field: 'nicDisplay', header: 'NIC', sortable: false, filterable: true, minWidth: '120px' },
    { field: 'epfNumber', header: 'EPF', sortable: true, filterable: true, minWidth: '100px' },
    { field: 'baseSalary', header: 'Salary', type: 'currency', align: 'right', sortable: true, minWidth: '120px' },
    { field: 'isActive', header: 'Status', type: 'boolean', align: 'center', sortable: true, minWidth: '100px' },
  ];

  companyFilter = '';
  branchFilter = '';
  costCenterFilter = '';
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];

  employeeToDelete: Employee | null = null;
  private destroy$ = new Subject<void>();

  // Debounce subject for global search
  private searchSubject = new Subject<string>();
  searchTerm = '';

  constructor(
    private employeesApi: EmployeesApiService,
    private organizationApi: OrganizationApiService,
    private router: Router,
    private authService: AuthService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.loadCompanies();
    this.searchSubject.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe(value => {
      this.searchTerm = value;
      this.page = 1; // Reset to first page on new search
      this.loadEmployees();
    });
    // Initial load
    this.loadEmployees();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCompanies(): void {
    this.organizationApi.getCompanies().subscribe(companies => {
      this.companies = companies;
      this.loadBranches();
      this.loadCostCenters();
    });
  }

  loadBranches(): void {
    this.organizationApi.getBranches(this.companyFilter || undefined).subscribe(branches => {
      this.branches = branches;
    });
  }

  loadCostCenters(): void {
    this.organizationApi
      .getCostCenters(this.companyFilter || undefined, this.branchFilter || undefined)
      .subscribe(costCenters => {
        this.costCenters = costCenters;
      });
  }

  loadEmployees(): void {
    this.loading = true;
    this.employeesApi
      .getEmployees(this.page, this.pageSize, {
        companyId: this.companyFilter || undefined,
        branchId: this.branchFilter || undefined,
        costCenterId: this.costCenterFilter || undefined,
        // searchTerm: this.searchTerm // Assuming API supports this, otherwise client-side filtering logic remains but API is preferred
      })
      .subscribe({
        next: (result: PaginatedResult<Employee>) => {
          const isAdmin = this.authService.isAdmin();
          this.employees = result.items.map(item => ({
            ...item,
            nicDisplay: isAdmin ? item.nicNumber : item.maskedNicNumber || '',
          }));
          this.totalCount = result.totalCount;
        },
        complete: () => {
          this.loading = false;
        }
      });
  }

  // Event Handlers for Power Grid
  onLazyLoad(event: any): void {
    // Calculate page from offset
    const first = event.first ?? 0;
    const rows = event.rows ?? 25;
    const newPage = Math.floor(first / rows) + 1;

    if (newPage !== this.page || rows !== this.pageSize) {
      this.page = newPage;
      this.pageSize = rows;
      this.loadEmployees();
    }
  }

  onGlobalSearch(value: string): void {
    this.searchSubject.next(value);
  }

  onCompanyChange(value: string | null): void {
    this.companyFilter = value ?? '';
    this.branchFilter = '';
    this.costCenterFilter = '';
    this.loadBranches();
    this.loadCostCenters();
    this.onScopeChange();
  }

  onBranchChange(value: string | null): void {
    this.branchFilter = value ?? '';
    this.costCenterFilter = '';
    this.loadCostCenters();
    this.onScopeChange();
  }

  onCostCenterChange(value: string | null): void {
    this.costCenterFilter = value ?? '';
    this.onScopeChange();
  }

  onScopeChange(): void {
    this.page = 1;
    this.loadEmployees();
  }

  // Actions
  goToCreate(): void {
    this.router.navigate(['/employees/new']);
  }

  viewEmployee(employee: Employee): void {
    this.router.navigate(['/employees', employee.id]);
  }

  editEmployee(employee: Employee): void {
    this.router.navigate(['/employees', employee.id, 'edit']);
  }

  confirmDelete(employee: Employee): void {
    this.employeeToDelete = employee;
    this.confirmationService.confirm({
      header: 'Delete Employee',
      icon: 'pi pi-exclamation-triangle',
      message: `Are you sure you want to delete ${employee.firstName} ${employee.lastName}?`,
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.deleteEmployee(),
      reject: () => {
        this.employeeToDelete = null;
      },
    });
  }

  deleteEmployee(): void {
    if (!this.employeeToDelete) {
      return;
    }
    const employee = this.employeeToDelete;
    this.employeesApi.deleteEmployee(employee.id).subscribe({
      next: () => {
        this.employeeToDelete = null;
        this.loadEmployees();
        this.messageService.add({
          severity: 'success',
          summary: 'Employee deleted',
          detail: `${employee.firstName} ${employee.lastName} was removed.`,
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Delete failed',
          detail: 'Unable to delete the employee. Please try again.',
        });
      },
    });
  }
}
