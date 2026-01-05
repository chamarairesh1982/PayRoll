import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EpfEtfRuleSet } from '../../models/epf-etf-rule-set.model';
import { EpfEtfRulesApiService } from '../../services/epf-etf-rules-api.service';

@Component({
  selector: 'app-epf-etf-rule-edit-page',
  templateUrl: './epf-etf-rule-edit-page.component.html',
  styleUrls: ['./epf-etf-rule-edit-page.component.scss'],
})
export class EpfEtfRuleEditPageComponent implements OnInit {
  ruleSet?: EpfEtfRuleSet;
  mode: 'create' | 'edit' = 'create';

  constructor(private epfEtfApi: EpfEtfRulesApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.mode = 'edit';
      this.epfEtfApi.getRuleSet(id).subscribe({
        next: data => (this.ruleSet = data),
        error: err => console.error('Failed to load EPF/ETF rule set', err),
      });
    }
  }

  handleSubmit(payload: Partial<EpfEtfRuleSet>): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id && id !== 'new') {
      this.epfEtfApi.updateRuleSet(id, payload).subscribe({
        next: () => this.router.navigate(['/config/epf-etf']),
        error: err => console.error('Failed to update EPF/ETF rule set', err),
      });
      return;
    }

    this.epfEtfApi.createRuleSet(payload).subscribe({
      next: () => this.router.navigate(['/config/epf-etf']),
      error: err => console.error('Failed to create EPF/ETF rule set', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/epf-etf']);
  }
}
