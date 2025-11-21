import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TaxRuleSet } from '../../models/tax-rule-set.model';
import { TaxRuleSetsApiService } from '../../services/tax-rule-sets-api.service';

@Component({
  selector: 'app-tax-rule-set-edit-page',
  templateUrl: './tax-rule-set-edit-page.component.html',
  styleUrls: ['./tax-rule-set-edit-page.component.scss'],
})
export class TaxRuleSetEditPageComponent implements OnInit {
  ruleSet?: TaxRuleSet;
  mode: 'create' | 'edit' = 'create';

  constructor(private taxApi: TaxRuleSetsApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.mode = 'edit';
      this.taxApi.getRuleSet(id).subscribe({
        next: data => (this.ruleSet = data),
        error: err => console.error('Failed to load tax rule set', err),
      });
    }
  }

  handleSubmit(payload: Partial<TaxRuleSet>): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id && id !== 'new') {
      this.taxApi.updateRuleSet(id, payload).subscribe({
        next: () => this.router.navigate(['/config/tax-rules']),
        error: err => console.error('Failed to update tax rule set', err),
      });
      return;
    }

    this.taxApi.createRuleSet(payload).subscribe({
      next: () => this.router.navigate(['/config/tax-rules']),
      error: err => console.error('Failed to create tax rule set', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/config/tax-rules']);
  }
}
