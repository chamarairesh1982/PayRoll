import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../services/employee.service';

@Component({
    selector: 'app-employee-form',
    templateUrl: './employee-form.component.html',
    styles: []
})
export class EmployeeFormComponent implements OnInit {
    form: FormGroup;
    isEditMode = false;
    employeeId: string | null = null;
    isLoading = false;

    constructor(
        private fb: FormBuilder,
        private employeeService: EmployeeService,
        private route: ActivatedRoute,
        private router: Router
    ) {
        this.form = this.fb.group({
            employeeCode: ['', [Validators.required, Validators.maxLength(50)]],
            firstName: ['', [Validators.required, Validators.maxLength(100)]],
            lastName: ['', [Validators.required, Validators.maxLength(100)]],
            email: ['', [Validators.required, Validators.email]],
            joinDate: ['', [Validators.required]],
            baseSalary: [0, [Validators.required, Validators.min(0)]]
        });
    }

    ngOnInit(): void {
        this.employeeId = this.route.snapshot.paramMap.get('id');
        if (this.employeeId && this.employeeId !== 'new') {
            this.isEditMode = true;
            this.loadEmployee(this.employeeId);
        }
    }

    loadEmployee(id: string): void {
        this.isLoading = true;
        this.employeeService.getEmployee(id).subscribe({
            next: (employee) => {
                // Format date for input type="date"
                const dateStr = employee.joinDate.split('T')[0];

                this.form.patchValue({
                    ...employee,
                    joinDate: dateStr
                });
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading employee', err);
                this.isLoading = false;
                // Handle error (e.g. redirect or show message)
            }
        });
    }

    onSubmit(): void {
        if (this.form.invalid) return;

        this.isLoading = true;
        const employeeData = this.form.value;

        if (this.isEditMode && this.employeeId) {
            this.employeeService.updateEmployee(this.employeeId, { id: this.employeeId, ...employeeData })
                .subscribe({
                    next: () => {
                        this.router.navigate(['/employees']);
                    },
                    error: (err) => {
                        console.error('Error updating employee', err);
                        this.isLoading = false;
                    }
                });
        } else {
            this.employeeService.createEmployee(employeeData)
                .subscribe({
                    next: () => {
                        this.router.navigate(['/employees']);
                    },
                    error: (err) => {
                        console.error('Error creating employee', err);
                        this.isLoading = false;
                    }
                });
        }
    }
}
