import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PaginatorState } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/services/auth.service';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee, PaginatedResult } from '../../models/employee.model';

type EmployeeRow = Employee & { nicDisplay: string };

@Component({
  selector: 'app-employees-list-page',
  templateUrl: './employees-list-page.component.html',
  styleUrls: ['./employees-list-page.component.scss'],
  providers: [ConfirmationService, MessageService],
})
export class EmployeesListPageComponent implements OnInit, OnDestroy {
  @ViewChild('employeesTable') employeesTable?: Table;

  employees: EmployeeRow[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 25;
  public searchTerm = '';
  companyFilter = '';
  branchFilter = '';
  costCenterFilter = '';
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];
  activeOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false },
  ];
  employeeToDelete: Employee | null = null;
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  constructor(
    private employeesApi: EmployeesApiService,
    private organizationApi: OrganizationApiService,
    private router: Router,
    private authService: AuthService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) {}

  ngOnInit(): void {
    this.loadCompanies();
    this.loadEmployees();
    this.searchSubject.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe(value => {
      this.searchTerm = value;
      this.applyGlobalFilter(value);
    });
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
    this.employeesApi
      .getEmployees(this.page, this.pageSize, {
        companyId: this.companyFilter || undefined,
        branchId: this.branchFilter || undefined,
        costCenterId: this.costCenterFilter || undefined,
      })
      .subscribe((result: PaginatedResult<Employee>) => {
        const isAdmin = this.authService.isAdmin();
        this.employees = result.items.map(item => ({
          ...item,
          nicDisplay: isAdmin ? item.nicNumber : item.maskedNicNumber || '',
        }));
        this.totalCount = result.totalCount;
        this.applyGlobalFilter(this.searchTerm);
      });
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

  public onPageChange(event: PaginatorState): void {
    const nextPage = (event.page ?? 0) + 1;
    const nextRows = event.rows ?? this.pageSize;
    const shouldReload = nextPage !== this.page || nextRows !== this.pageSize;
    this.page = nextPage;
    this.pageSize = nextRows;
    if (shouldReload) {
      this.loadEmployees();
    }
  }

  onScopeChange(): void {
    this.page = 1;
    this.loadEmployees();
  }

  public onGlobalSearch(value: string): void {
    this.searchSubject.next(value);
  }

  private applyGlobalFilter(value: string): void {
    if (this.employeesTable) {
      this.employeesTable.filterGlobal(value, 'contains');
    }
  }
}
