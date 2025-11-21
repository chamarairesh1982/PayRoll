import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { LeaveRequest } from '../../models/leave-request.model';
import { LeaveRequestsApiService } from '../../services/leave-requests-api.service';

@Component({
  selector: 'app-leave-create-page',
  templateUrl: './leave-create-page.component.html',
  styleUrls: ['./leave-create-page.component.scss'],
})
export class LeaveCreatePageComponent {
  constructor(private leaveRequestsApi: LeaveRequestsApiService, private router: Router) {}

  onSubmitted(payload: Partial<LeaveRequest>): void {
    this.leaveRequestsApi.createLeaveRequest(payload).subscribe({
      next: () => this.router.navigate(['/leave']),
      error: err => console.error('Failed to create leave request', err),
    });
  }
}
