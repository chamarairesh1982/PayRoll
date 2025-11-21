import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequest } from '../../models/leave-request.model';
import { LeaveRequestsApiService } from '../../services/leave-requests-api.service';

@Component({
  selector: 'app-leave-edit-page',
  templateUrl: './leave-edit-page.component.html',
  styleUrls: ['./leave-edit-page.component.scss'],
})
export class LeaveEditPageComponent implements OnInit {
  leaveRequest?: LeaveRequest;
  isLoading = false;

  constructor(private route: ActivatedRoute, private leaveRequestsApi: LeaveRequestsApiService, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isLoading = true;
      this.leaveRequestsApi.getLeaveRequest(id).subscribe({
        next: request => {
          this.leaveRequest = request;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load leave request', err);
          this.isLoading = false;
        },
      });
    }
  }

  onSubmitted(payload: Partial<LeaveRequest>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.leaveRequestsApi.updateLeaveRequest(id, payload).subscribe({
      next: () => this.router.navigate(['/leave']),
      error: err => console.error('Failed to update leave request', err),
    });
  }
}
