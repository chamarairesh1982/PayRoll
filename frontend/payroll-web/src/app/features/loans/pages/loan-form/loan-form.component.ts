import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { Employee } from '../../../employees/models/employee.model';
import { LoanType, LoanStatus } from '../../models/loan.model';
import { LoansApiService } from '../../services/loans-api.service';

@Component({
    selector: 'app-loan-form',
    templateUrl: './loan-form.component.html',
    styleUrls: ['./loan-form.component.scss'],
})
export class LoanFormComponent implements OnInit {
    loanForm: FormGroup;
    isEdit = false;
    loanId?: string;
    employees: Employee[] = [];

    loanTypes: { label: string; value: LoanType }[] = [
        { label: 'Company Loan', value: 'Company Loan' },
        { label: 'Salary Advance', value: 'Salary Advance' },
        { label: 'Personal Loan', value: 'Personal Loan' },
        { label: 'Emergency Relief', value: 'Emergency Relief' },
    ];

    statusOptions: { label: string; value: LoanStatus }[] = [
        { label: 'Pending', value: 'Pending' },
        { label: 'Active', value: 'Active' },
        { label: 'Settled', value: 'Settled' },
        { label: 'Defaulted', value: 'Defaulted' },
    ];

    constructor(
        private fb: FormBuilder,
        private loansApi: LoansApiService,
        private employeesApi: EmployeesApiService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService
    ) {
        this.loanForm = this.fb.group({
            employeeId: [null, Validators.required],
            loanType: ['Company Loan', Validators.required],
            principal: [0, [Validators.required, Validators.min(1)]],
            interestRate: [0, [Validators.required, Validators.min(0)]],
            installmentAmount: [0, [Validators.required, Validators.min(1)]],
            totalRepayable: [0, [Validators.required, Validators.min(1)]],
            disbursementDate: [new Date(), Validators.required],
            status: ['Pending', Validators.required],
            remarks: [''],
        });
    }

    ngOnInit(): void {
        this.loadEmployees();
        this.loanId = this.route.snapshot.params['id'];
        if (this.loanId) {
            this.isEdit = true;
            this.loadLoan();
        }
    }

    loadEmployees(): void {
        this.employeesApi.getEmployees(0, 1000).subscribe((res) => {
            this.employees = res.items;
        });
    }

    loadLoan(): void {
        if (!this.loanId) return;
        this.loansApi.getLoan(this.loanId).subscribe((loan) => {
            this.loanForm.patchValue({
                ...loan,
                disbursementDate: new Date(loan.disbursementDate),
            });
        });
    }

    onSubmit(): void {
        if (this.loanForm.invalid) {
            this.loanForm.markAllAsTouched();
            return;
        }

        const payload = this.loanForm.value;
        const request = this.isEdit
            ? this.loansApi.createLoan({ ...payload, id: this.loanId }) // Update logic might need separate method if API differs
            : this.loansApi.createLoan(payload);

        request.subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'Protocol Executed',
                    detail: `Loan facility ${this.isEdit ? 'updated' : 'initialized'} successfully.`,
                });
                this.router.navigate(['/loans']);
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Transmission Error',
                    detail: 'Unable to commit loan configuration to the ledger.',
                });
            },
        });
    }

    onCancel(): void {
        this.router.navigate(['/loans']);
    }
}
