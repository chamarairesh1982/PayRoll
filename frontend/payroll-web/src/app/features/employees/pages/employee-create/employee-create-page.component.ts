import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employee-create-page',
  templateUrl: './employee-create-page.component.html',
  styleUrls: ['./employee-create-page.component.scss'],
})
export class EmployeeCreatePageComponent {
  constructor(
    private employeesApi: EmployeesApiService,
    private router: Router,
    private messageService: MessageService,
  ) {}

  handleSubmit(payload: Partial<Employee>): void {
    this.employeesApi.createEmployee(payload).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Employee created',
          detail: 'The employee profile has been saved.',
        });
        this.router.navigate(['/employees']);
      },
      error: err => {
        console.error('Failed to create employee', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Create failed',
          detail: 'Unable to save the employee. Please try again.',
        });
      },
    });
  }

  public handleCancel(): void {
    this.router.navigate(['/employees']);
  }
}
