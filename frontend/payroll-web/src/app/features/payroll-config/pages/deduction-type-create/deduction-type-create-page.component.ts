import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { DeductionType } from '../../models/deduction-type.model';
import { DeductionTypesApiService } from '../../services/deduction-types-api.service';

@Component({
  selector: 'app-deduction-type-create-page',
  templateUrl: './deduction-type-create-page.component.html',
  styleUrls: ['./deduction-type-create-page.component.scss'],
})
export class DeductionTypeCreatePageComponent {
  constructor(private deductionTypesApi: DeductionTypesApiService, private router: Router) {}

  handleSubmit(payload: Partial<DeductionType>): void {
    this.deductionTypesApi.createDeductionType(payload).subscribe({
      next: () => this.router.navigate(['/config/deductions']),
      error: err => console.error('Failed to create deduction type', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/deductions']);
  }
}
