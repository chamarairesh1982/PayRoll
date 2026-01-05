import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Bank } from '../../models/bank.model';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-bank-detail-page',
  templateUrl: './bank-detail-page.component.html',
  styleUrls: ['./bank-detail-page.component.scss'],
})
export class BankDetailPageComponent implements OnInit {
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

  goBack(): void {
    this.router.navigate(['/config/banks']);
  }

  editBank(): void {
    if (!this.bank) {
      return;
    }
    this.router.navigate(['/config/banks', this.bank.id, 'edit']);
  }
}
