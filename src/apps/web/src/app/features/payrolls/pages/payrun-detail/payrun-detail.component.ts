import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PayrollService, PayRunDetail } from '../../services/payroll.service';

@Component({
    selector: 'app-payrun-detail',
    templateUrl: './payrun-detail.component.html',
    styles: []
})
export class PayRunDetailComponent implements OnInit {
    payRun: PayRunDetail | null = null;
    isLoading = false;

    constructor(
        private route: ActivatedRoute,
        private payrollService: PayrollService
    ) { }

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.loadPayRun(id);
        }
    }

    loadPayRun(id: string): void {
        this.isLoading = true;
        this.payrollService.getPayRun(id).subscribe({
            next: (data) => {
                this.payRun = data;
                this.isLoading = false;
            },
            error: (err) => {
                console.error(err);
                this.isLoading = false;
            }
        });
    }

    downloadPayslip(employeeId: string): void {
        if (!this.payRun) return;

        this.payrollService.downloadPayslip(this.payRun.id, employeeId).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `Payslip-${employeeId}.pdf`;
                link.click();
                window.URL.revokeObjectURL(url);
            },
            error: (err) => {
                console.error('Failed to download payslip', err);
                alert('Failed to download payslip');
            }
        });
    }
}
