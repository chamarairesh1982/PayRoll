import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Bank } from '../../models/bank.model';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-bank-edit-page',
  templateUrl: './bank-edit-page.component.html',
  styleUrls: ['./bank-edit-page.component.scss'],
})
export class BankEditPageComponent implements OnInit {
  bank?: Bank;

  constructor(private banksApi: BanksApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.banksApi.getBank(id).subscribe({
        next: bank => (this.bank = bank),
        error: err => console.error('Failed to load bank', err),
      });
    }
  }

  handleSubmit(payload: Partial<Bank>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.banksApi.updateBank(id, payload).subscribe({
      next: () => this.router.navigate(['/config/banks']),
      error: err => console.error('Failed to update bank', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/banks']);
  }
}
