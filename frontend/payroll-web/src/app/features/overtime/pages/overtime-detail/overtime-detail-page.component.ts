import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OvertimeRecord } from '../../models/overtime-record.model';
import { OvertimeApiService } from '../../services/overtime-api.service';

@Component({
  selector: 'app-overtime-detail-page',
  templateUrl: './overtime-detail-page.component.html',
  styleUrls: ['./overtime-detail-page.component.scss'],
})
export class OvertimeDetailPageComponent implements OnInit {
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

  goBack(): void {
    this.router.navigate(['/overtime']);
  }

  goToEdit(): void {
    if (this.overtimeRecord && !this.overtimeRecord.isLockedForPayroll) {
      this.router.navigate(['/overtime', this.overtimeRecord.id, 'edit']);
    }
  }
}
