import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employee-edit-page',
  templateUrl: './employee-edit-page.component.html',
  styleUrls: ['./employee-edit-page.component.scss'],
})
export class EmployeeEditPageComponent implements OnInit {
  employee?: Employee;

  constructor(private employeesApi: EmployeesApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.employeesApi.getEmployee(id).subscribe(employee => (this.employee = employee));
    }
  }

  handleSubmit(payload: Partial<Employee>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }
    this.employeesApi.updateEmployee(id, payload).subscribe({
      next: () => this.router.navigate(['/employees', id]),
      error: err => console.error('Failed to update employee', err),
    });
  }
}
