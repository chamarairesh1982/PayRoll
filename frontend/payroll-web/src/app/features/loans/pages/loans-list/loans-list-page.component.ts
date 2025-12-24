import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Subject, takeUntil } from 'rxjs';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';
import { Loan } from '../../models/loan.model';
import { LoansApiService } from '../../services/loans-api.service';

@Component({
    selector: 'app-loans-list-page',
    templateUrl: './loans-list-page.component.html',
    styleUrls: ['./loans-list-page.component.scss'],
})
export class LoansListPageComponent implements OnInit, OnDestroy {
    columns: DataTableColumn<Loan>[] = [
        { field: 'employeeName', header: 'Staff Identity', sortable: true, minWidth: '200px' },
        { field: 'loanType', header: 'Product Spec', sortable: true, minWidth: '150px' },
        { field: 'principal', header: 'Principal', type: 'amount', sortable: true, minWidth: '130px' },
        { field: 'outstanding', header: 'Current Exposure', type: 'amount', sortable: true, minWidth: '150px' },
        { field: 'repaidAmount', header: 'Capital Repaid', type: 'amount', sortable: true, minWidth: '150px' },
        { field: 'interestRate', header: 'Yield %', type: 'number', sortable: true, minWidth: '100px' },
        { field: 'status', header: 'Risk Status', type: 'status', sortable: true, minWidth: '130px' },
        { field: 'nextInstallmentDate', header: 'Next Recovery', type: 'date', sortable: true, minWidth: '140px' },
    ];

    loans: Loan[] = [];
    isLoading = false;
    totalRecords = 0;

    private destroy$ = new Subject<void>();

    constructor(
        private loansApi: LoansApiService,
        private router: Router,
        private messageService: MessageService
    ) { }

    ngOnInit(): void {
        this.loadLoans();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadLoans(event?: any): void {
        this.isLoading = true;
        const page = event ? event.first / event.rows : 0;
        const pageSize = event ? event.rows : 10;

        this.loansApi
            .getLoans({ page, pageSize })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.loans = result.items;
                    this.totalRecords = result.totalCount;
                    this.isLoading = false;
                },
                error: () => {
                    this.messageService.add({
                        severity: 'error',
                        summary: 'Data Retrieval Failure',
                        detail: 'Unable to synchronize loan infrastructure data.',
                    });
                    this.isLoading = false;
                },
            });
    }

    onCreateLoan(): void {
        this.router.navigate(['/loans/new']);
    }

    onViewLoan(loan: Loan): void {
        this.router.navigate(['/loans', loan.id]);
    }

    getProgress(loan: Loan): number {
        if (loan.totalRepayable === 0) return 0;
        return Math.round((loan.repaidAmount / loan.totalRepayable) * 100);
    }

    getStatusSeverity(status: string): string {
        switch (status) {
            case 'Settled':
                return 'success';
            case 'Active':
            case 'Disbursed':
                return 'info';
            case 'Pending':
                return 'warning';
            case 'Defaulted':
                return 'danger';
            default:
                return 'secondary';
        }
    }
}
