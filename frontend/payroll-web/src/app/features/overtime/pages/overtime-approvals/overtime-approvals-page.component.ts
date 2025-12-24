import { Component, OnInit } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { OTEntry } from '../../models/ot-entry.model';
import { OvertimeApiService } from '../../services/overtime-api.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-overtime-approvals-page',
  templateUrl: './overtime-approvals-page.component.html',
  styleUrls: ['./overtime-approvals-page.component.scss'],
})
export class OvertimeApprovalsPageComponent implements OnInit {
  records: OTEntry[] = [];
  isLoading = false;
  error: string | null = null;

  columns: DataTableColumn<OTEntry>[] = [
    { field: 'workDate', header: 'Work Date', sortable: true, type: 'date', minWidth: '120px' },
    { field: 'employeeName', header: 'Employee', sortable: true, filterable: true, minWidth: '200px' },
    { field: 'rawMinutes', header: 'Minutes', sortable: true, type: 'number', minWidth: '100px' },
    { field: 'type', header: 'Type', sortable: true, minWidth: '120px' },
    { field: 'comment', header: 'Comments', minWidth: '200px' },
  ];

  constructor(
    private overtimeApi: OvertimeApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.loadRecords();
  }

  loadRecords(): void {
    this.isLoading = true;
    this.error = null;
    this.overtimeApi
      .getOvertimeRecords({
        page: 1,
        pageSize: 100,
        status: 'Submitted',
      })
      .subscribe({
        next: result => {
          this.records = result.items;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load submitted overtime records', err);
          this.error = err.error?.message || 'Failed to load submitted overtime records.';
          this.isLoading = false;
        },
      });
  }

  approve(record: OTEntry): void {
    this.confirmationService.confirm({
      message: `Confirm approval of overtime session for ${record.employeeName || record.employeeId}?`,
      header: 'Commit Approval',
      icon: 'pi pi-check-circle',
      accept: () => {
        this.overtimeApi.approveOvertimeRecord(record.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Approved', detail: 'Overtime session authorized.' });
            this.loadRecords();
          },
          error: err => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Authorization failed.' });
          },
        });
      },
    });
  }

  reject(record: OTEntry): void {
    this.confirmationService.confirm({
      message: `Decline overtime session for ${record.employeeName || record.employeeId}?`,
      header: 'Reject Entry',
      icon: 'pi pi-times-circle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.overtimeApi.rejectOvertimeRecord(record.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'warn', summary: 'Rejected', detail: 'Overtime session declined.' });
            this.loadRecords();
          },
          error: err => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Action failed.' });
          },
        });
      },
    });
  }
}
