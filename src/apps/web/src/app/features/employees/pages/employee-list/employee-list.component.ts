import { Component, OnInit } from '@angular/core';
import { Employee, EmployeeService } from '../../services/employee.service';

@Component({
    selector: 'app-employee-list',
    templateUrl: './employee-list.component.html',
    styles: []
})
export class EmployeeListComponent implements OnInit {
    employees: Employee[] = [];
    isLoading = false;

    constructor(private employeeService: EmployeeService) { }

    ngOnInit(): void {
        this.loadEmployees();
    }

    loadEmployees(): void {
        this.isLoading = true;
        this.employeeService.getEmployees().subscribe({
            next: (data) => {
                this.employees = data;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading employees', err);
                this.isLoading = false;
            }
        });
    }

    deleteEmployee(id: string): void {
        if (confirm('Are you sure you want to delete this employee?')) {
            this.employeeService.deleteEmployee(id).subscribe(() => {
                this.loadEmployees();
            });
        }
    }
}
