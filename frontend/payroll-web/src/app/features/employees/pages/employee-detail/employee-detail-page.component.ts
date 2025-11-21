import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeesApiService } from '../../services/employees-api.service';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employee-detail-page',
  templateUrl: './employee-detail-page.component.html',
  styleUrls: ['./employee-detail-page.component.scss'],
})
export class EmployeeDetailPageComponent implements OnInit {
  employee?: Employee;

  constructor(private route: ActivatedRoute, private router: Router, private employeesApi: EmployeesApiService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.employeesApi.getEmployee(id).subscribe(employee => (this.employee = employee));
    }
  }

  goBack(): void {
    this.router.navigate(['/employees']);
  }

  editEmployee(): void {
    if (this.employee) {
      this.router.navigate(['/employees', this.employee.id, 'edit']);
    }
  }
}
