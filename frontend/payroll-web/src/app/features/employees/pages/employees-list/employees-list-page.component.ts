import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee, PaginatedResult } from '../../models/employee.model';

type EmployeeRow = Employee & { fullName: string };

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
  columns = [
    { field: 'employeeCode' as const, header: 'Employee Code' },
    { field: 'fullName' as const, header: 'Name' },
    { field: 'nicNumber' as const, header: 'NIC' },
    { field: 'employmentStartDate' as const, header: 'Employment Start' },
    { field: 'baseSalary' as const, header: 'Base Salary' },
    { field: 'isActive' as const, header: 'Active' },
  ];

  showConfirm = false;
  employeeToDelete: Employee | null = null;

  constructor(private employeesApi: EmployeesApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.employeesApi.getEmployees(this.page, this.pageSize).subscribe((result: PaginatedResult<Employee>) => {
      this.employees = result.items.map(item => ({ ...item, fullName: `${item.firstName} ${item.lastName}` }));
      this.totalCount = result.totalCount;
    });
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
}
