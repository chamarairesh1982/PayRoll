import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OvertimeRecord } from '../../models/overtime-record.model';
import { OvertimeApiService } from '../../services/overtime-api.service';

@Component({
  selector: 'app-overtime-edit-page',
  templateUrl: './overtime-edit-page.component.html',
  styleUrls: ['./overtime-edit-page.component.scss'],
})
export class OvertimeEditPageComponent implements OnInit {
  overtimeRecord?: OvertimeRecord;
  isLoading = false;

  constructor(private route: ActivatedRoute, private overtimeApi: OvertimeApiService, private router: Router) {}

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

  onSubmitted(payload: Partial<OvertimeRecord>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.overtimeApi.updateOvertimeRecord(id, payload).subscribe({
      next: () => this.router.navigate(['/overtime']),
      error: err => console.error('Failed to update overtime record', err),
    });
  }
}
