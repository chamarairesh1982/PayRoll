import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Bank } from '../../models/bank.model';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-bank-create-page',
  templateUrl: './bank-create-page.component.html',
  styleUrls: ['./bank-create-page.component.scss'],
})
export class BankCreatePageComponent {
  constructor(private banksApi: BanksApiService, private router: Router) {}

  handleSubmit(payload: Partial<Bank>): void {
    this.banksApi.createBank(payload).subscribe({
      next: () => this.router.navigate(['/config/banks']),
      error: err => console.error('Failed to create bank', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/banks']);
  }
}
