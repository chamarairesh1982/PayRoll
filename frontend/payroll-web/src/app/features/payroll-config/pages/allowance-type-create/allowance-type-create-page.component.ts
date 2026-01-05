import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AllowanceTypesApiService } from '../../services/allowance-types-api.service';
import { AllowanceType } from '../../models/allowance-type.model';

@Component({
  selector: 'app-allowance-type-create-page',
  templateUrl: './allowance-type-create-page.component.html',
  styleUrls: ['./allowance-type-create-page.component.scss'],
})
export class AllowanceTypeCreatePageComponent {
  constructor(private allowanceTypesApi: AllowanceTypesApiService, private router: Router) {}

  handleSubmit(payload: Partial<AllowanceType>): void {
    this.allowanceTypesApi.createAllowanceType(payload).subscribe({
      next: () => this.router.navigate(['/config/allowances']),
      error: err => console.error('Failed to create allowance type', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/allowances']);
  }
}
