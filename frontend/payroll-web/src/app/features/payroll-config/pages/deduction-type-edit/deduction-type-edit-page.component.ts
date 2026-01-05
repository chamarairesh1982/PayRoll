import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DeductionType } from '../../models/deduction-type.model';
import { DeductionTypesApiService } from '../../services/deduction-types-api.service';

@Component({
  selector: 'app-deduction-type-edit-page',
  templateUrl: './deduction-type-edit-page.component.html',
  styleUrls: ['./deduction-type-edit-page.component.scss'],
})
export class DeductionTypeEditPageComponent implements OnInit {
  deductionType?: DeductionType;

  constructor(private deductionTypesApi: DeductionTypesApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.deductionTypesApi.getDeductionType(id).subscribe({
        next: deductionType => (this.deductionType = deductionType),
        error: err => console.error('Failed to load deduction type', err),
      });
    }
  }

  handleSubmit(payload: Partial<DeductionType>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.deductionTypesApi.updateDeductionType(id, payload).subscribe({
      next: () => this.router.navigate(['/config/deductions']),
      error: err => console.error('Failed to update deduction type', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/deductions']);
  }
}
