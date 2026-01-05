import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ApprovalService } from '../../../approvals/services/approval.service';
import { OTEntry } from '../../models/ot-entry.model';
import { OvertimeApiService } from '../../services/overtime-api.service';

@Component({
  selector: 'app-overtime-detail-page',
  templateUrl: './overtime-detail-page.component.html',
  styleUrls: ['./overtime-detail-page.component.scss'],
})
export class OvertimeDetailPageComponent implements OnInit {
  overtimeRecord?: OTEntry;
  isLoading = false;

  constructor(
    private route: ActivatedRoute,
    private overtimeApi: OvertimeApiService,
    private router: Router,
    private approvalService: ApprovalService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isLoading = true;
      this.overtimeApi.getOvertimeRecord(id).subscribe({
        next: record => {
          this.overtimeRecord = record;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load overtime record', err);
          this.isLoading = false;
        },
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/overtime']);
  }

  goToEdit(): void {
    if (this.overtimeRecord && !this.overtimeRecord.isLockedForPayroll && this.overtimeRecord.status === 'Draft') {
      this.router.navigate(['/overtime', this.overtimeRecord.id, 'edit']);
    }
  }

  submitRecord(): void {
    if (!this.overtimeRecord || this.overtimeRecord.isLockedForPayroll || this.overtimeRecord.status !== 'Draft') {
      return;
    }

    this.overtimeApi.submitOvertimeRecord(this.overtimeRecord.id).subscribe({
      next: () => this.ngOnInit(),
      error: err => console.error('Failed to submit overtime record', err),
    });
  }

  submitForApproval(): void {
    if (!this.overtimeRecord) {
      return;
    }
    this.approvalService.submitRequest({
      type: 'Overtime',
      employee: this.overtimeRecord.employeeName || this.overtimeRecord.employeeCode || this.overtimeRecord.employeeId,
      payload: {
        workDate: this.overtimeRecord.workDate,
        minutes: this.overtimeRecord.rawMinutes,
        type: this.overtimeRecord.type,
        comment: this.overtimeRecord.comment,
      },
    });
    this.messageService.add({
      severity: 'success',
      summary: 'Submitted',
      detail: 'Overtime entry sent for approval.',
    });
  }
}
