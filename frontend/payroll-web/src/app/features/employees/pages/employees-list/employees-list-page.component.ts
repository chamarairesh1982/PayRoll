import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee, PaginatedResult } from '../../models/employee.model';

type EmployeeRow = Employee & { fullName: string; nicDisplay: string };

@Component({
  selector: 'app-employees-list-page',
  templateUrl: './employees-list-page.component.html',
  styleUrls: ['./employees-list-page.component.scss'],
})
export class EmployeesListPageComponent implements OnInit {
  employees: EmployeeRow[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 25;
  companyFilter = '';
  branchFilter = '';
  costCenterFilter = '';
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];
  columns = [
    { field: 'employeeCode' as const, header: 'Employee Code' },
    { field: 'fullName' as const, header: 'Name' },
    { field: 'nicDisplay' as const, header: 'NIC' },
    { field: 'epfNumber' as const, header: 'EPF Number' },
    { field: 'employmentStartDate' as const, header: 'Employment Start' },
    { field: 'baseSalary' as const, header: 'Base Salary' },
    { field: 'isActive' as const, header: 'Active' },
  ];

  showConfirm = false;
  employeeToDelete: Employee | null = null;

  constructor(
    private employeesApi: EmployeesApiService,
    private organizationApi: OrganizationApiService,
    private router: Router,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.loadCompanies();
    this.loadEmployees();
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
          fullName: `${item.firstName} ${item.lastName}`,
          nicDisplay: isAdmin ? item.nicNumber : item.maskedNicNumber || '',
        }));
        this.totalCount = result.totalCount;
      });
  }

  onCompanyChange(value: string): void {
    this.companyFilter = value;
    this.branchFilter = '';
    this.costCenterFilter = '';
    this.loadBranches();
    this.loadCostCenters();
    this.onScopeChange();
  }

  onBranchChange(value: string): void {
    this.branchFilter = value;
    this.costCenterFilter = '';
    this.loadCostCenters();
    this.onScopeChange();
  }

  onCostCenterChange(value: string): void {
    this.costCenterFilter = value;
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
    this.showConfirm = true;
  }

  cancelDelete(): void {
    this.employeeToDelete = null;
    this.showConfirm = false;
  }

  deleteEmployee(): void {
    if (!this.employeeToDelete) {
      return;
    }
    this.employeesApi.deleteEmployee(this.employeeToDelete.id).subscribe(() => {
      this.cancelDelete();
      this.loadEmployees();
    });
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.loadEmployees();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadEmployees();
    }
  }

  onScopeChange(): void {
    this.page = 1;
    this.loadEmployees();
  }
}
