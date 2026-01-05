import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AllowanceType } from '../../models/allowance-type.model';
import { AllowanceTypesApiService } from '../../services/allowance-types-api.service';

@Component({
  selector: 'app-allowance-type-edit-page',
  templateUrl: './allowance-type-edit-page.component.html',
  styleUrls: ['./allowance-type-edit-page.component.scss'],
})
export class AllowanceTypeEditPageComponent implements OnInit {
  allowanceType?: AllowanceType;

  constructor(private allowanceTypesApi: AllowanceTypesApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.allowanceTypesApi.getAllowanceType(id).subscribe({
        next: allowanceType => (this.allowanceType = allowanceType),
        error: err => console.error('Failed to load allowance type', err),
      });
    }
  }

  handleSubmit(payload: Partial<AllowanceType>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.allowanceTypesApi.updateAllowanceType(id, payload).subscribe({
      next: () => this.router.navigate(['/config/allowances']),
      error: err => console.error('Failed to update allowance type', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/allowances']);
  }
}
