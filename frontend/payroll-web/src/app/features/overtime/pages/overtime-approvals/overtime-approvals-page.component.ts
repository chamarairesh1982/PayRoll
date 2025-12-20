import { Component, OnInit } from '@angular/core';
import { OTEntry } from '../../models/ot-entry.model';
import { OvertimeApiService } from '../../services/overtime-api.service';

@Component({
  selector: 'app-overtime-approvals-page',
  templateUrl: './overtime-approvals-page.component.html',
  styleUrls: ['./overtime-approvals-page.component.scss'],
})
export class OvertimeApprovalsPageComponent implements OnInit {
  records: OTEntry[] = [];
  isLoading = false;
  error: string | null = null;

  constructor(private overtimeApi: OvertimeApiService) {}

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
    const comment = window.prompt('Approval comment (optional)');
    this.overtimeApi.approveOvertimeRecord(record.id, comment).subscribe({
      next: () => this.loadRecords(),
      error: err => {
        console.error('Failed to approve overtime record', err);
        this.error = err.error?.message || 'Failed to approve overtime record.';
      },
    });
  }

  reject(record: OTEntry): void {
    const comment = window.prompt('Rejection comment (optional)');
    this.overtimeApi.rejectOvertimeRecord(record.id, comment).subscribe({
      next: () => this.loadRecords(),
      error: err => {
        console.error('Failed to reject overtime record', err);
        this.error = err.error?.message || 'Failed to reject overtime record.';
      },
    });
  }
}
