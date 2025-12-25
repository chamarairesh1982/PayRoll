import { Component, OnInit } from '@angular/core';
import { PayrollService, PayRun } from '../../services/payroll.service';

@Component({
    selector: 'app-payrun-list',
    templateUrl: './payrun-list.component.html',
    styles: []
})
export class PayRunListComponent implements OnInit {
    payRuns: PayRun[] = [];
    isLoading = false;
    showCreateModal = false;

    newPayRun = {
        name: '',
        periodStart: '',
        periodEnd: '',
        paymentDate: ''
    };

    constructor(private payrollService: PayrollService) { }

    ngOnInit(): void {
        this.loadPayRuns();
    }

    loadPayRuns(): void {
        this.isLoading = true;
        this.payrollService.getPayRuns().subscribe({
            next: (data) => {
                this.payRuns = data;
                this.isLoading = false;
            },
            error: (err) => {
                console.error(err);
                this.isLoading = false;
            }
        });
    }

    createPayRun(): void {
        if (!this.newPayRun.name || !this.newPayRun.periodStart) return;

        this.isLoading = true;
        this.payrollService.createPayRun(this.newPayRun).subscribe({
            next: () => {
                this.showCreateModal = false;
                this.loadPayRuns();
                this.newPayRun = { name: '', periodStart: '', periodEnd: '', paymentDate: '' };
            },
            error: (err) => {
                console.error(err);
                this.isLoading = false;
                alert('Failed to create pay run');
            }
        });
    }
}
