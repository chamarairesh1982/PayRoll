import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Bank } from '../../models/bank.model';
import { BankBranch } from '../../models/bank-branch.model';
import { BankBranchesApiService } from '../../services/bank-branches-api.service';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-bank-branch-create-page',
  templateUrl: './bank-branch-create-page.component.html',
  styleUrls: ['./bank-branch-create-page.component.scss'],
})
export class BankBranchCreatePageComponent implements OnInit {
  banks: Bank[] = [];

  constructor(
    private bankBranchesApi: BankBranchesApiService,
    private banksApi: BanksApiService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadBanks();
  }

  loadBanks(): void {
    this.banksApi.getBanks({ page: 1, pageSize: 200, isActive: true }).subscribe({
      next: result => (this.banks = result.items),
      error: err => console.error('Failed to load banks', err),
    });
  }

  handleSubmit(payload: Partial<BankBranch>): void {
    this.bankBranchesApi.createBankBranch(payload).subscribe({
      next: () => this.router.navigate(['/config/bank-branches']),
      error: err => console.error('Failed to create bank branch', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/bank-branches']);
  }
}
