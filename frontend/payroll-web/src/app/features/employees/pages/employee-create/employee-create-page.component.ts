import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employee-create-page',
  templateUrl: './employee-create-page.component.html',
  styleUrls: ['./employee-create-page.component.scss'],
})
export class EmployeeCreatePageComponent {
  constructor(private employeesApi: EmployeesApiService, private router: Router) {}

  handleSubmit(payload: Partial<Employee>): void {
    this.employeesApi.createEmployee(payload).subscribe({
      next: () => this.router.navigate(['/employees']),
      error: err => console.error('Failed to create employee', err),
    });
  }
}
