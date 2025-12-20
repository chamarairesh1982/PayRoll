import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Bank } from '../../models/bank.model';
import { BankBranch } from '../../models/bank-branch.model';
import { BankBranchesApiService } from '../../services/bank-branches-api.service';
import { BanksApiService } from '../../services/banks-api.service';

@Component({
  selector: 'app-bank-branch-edit-page',
  templateUrl: './bank-branch-edit-page.component.html',
  styleUrls: ['./bank-branch-edit-page.component.scss'],
})
export class BankBranchEditPageComponent implements OnInit {
  branch?: BankBranch;
  banks: Bank[] = [];

  constructor(
    private bankBranchesApi: BankBranchesApiService,
    private banksApi: BanksApiService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadBanks();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.bankBranchesApi.getBankBranch(id).subscribe({
        next: branch => (this.branch = branch),
        error: err => console.error('Failed to load bank branch', err),
      });
    }
  }

  loadBanks(): void {
    this.banksApi.getBanks({ page: 1, pageSize: 200, isActive: null }).subscribe({
      next: result => (this.banks = result.items),
      error: err => console.error('Failed to load banks', err),
    });
  }

  handleSubmit(payload: Partial<BankBranch>): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.bankBranchesApi.updateBankBranch(id, payload).subscribe({
      next: () => this.router.navigate(['/config/bank-branches']),
      error: err => console.error('Failed to update bank branch', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/bank-branches']);
  }
}
