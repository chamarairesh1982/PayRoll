import { Component, OnInit } from '@angular/core';
import { OTRule, OTRulePayload } from '../../models/ot-rule.model';
import { OvertimeConfigApiService } from '../../services/overtime-config-api.service';

@Component({
  selector: 'app-overtime-settings-page',
  templateUrl: './overtime-settings-page.component.html',
  styleUrls: ['./overtime-settings-page.component.scss'],
})
export class OvertimeSettingsPageComponent implements OnInit {
  rules: OTRule[] = [];
  selectedRule: OTRule | null = null;
  isLoading = true;
  isSaving = false;
  error: string | null = null;
  successMessage: string | null = null;

  constructor(private overtimeConfigApi: OvertimeConfigApiService) {}

  ngOnInit(): void {
    this.loadRules();
  }

  loadRules(): void {
    this.isLoading = true;
    this.error = null;

    this.overtimeConfigApi.getRules().subscribe({
      next: rules => {
        this.rules = rules;
        if (!this.selectedRule && rules.length) {
          this.selectedRule = rules[0];
        }
        this.isLoading = false;
      },
      error: err => {
        console.error('Failed to load overtime rules', err);
        this.error = err.error?.message || 'Failed to load overtime rules.';
        this.isLoading = false;
      },
    });
  }

  selectRule(rule: OTRule): void {
    this.selectedRule = rule;
    this.successMessage = null;
  }

  createRule(): void {
    this.selectedRule = {
      id: '',
      name: '',
      weekdayMultiplier: 1.5,
      weekendMultiplier: 2,
      holidayMultiplier: 2,
      roundingMinutes: 15,
      dailyCapHours: 12,
      payRunCapHours: 80,
      appliesOnWeekend: true,
      appliesOnHoliday: true,
      isActive: true,
    };
    this.successMessage = null;
  }

  saveRule(payload: OTRulePayload): void {
    this.isSaving = true;
    this.successMessage = null;
    this.error = null;

    if (this.selectedRule?.id) {
      this.overtimeConfigApi.updateRule(this.selectedRule.id, payload).subscribe({
        next: rule => {
          this.rules = this.rules.map(existing => (existing.id === rule.id ? rule : existing));
          this.selectedRule = rule;
          this.successMessage = 'Overtime rule updated successfully.';
          this.isSaving = false;
        },
        error: err => {
          console.error('Failed to update overtime rule', err);
          this.error = err.error?.message || 'Failed to update overtime rule.';
          this.isSaving = false;
        },
      });
      return;
    }

    this.overtimeConfigApi.createRule(payload).subscribe({
      next: rule => {
        this.rules = [...this.rules, rule];
        this.selectedRule = rule;
        this.successMessage = 'Overtime rule created successfully.';
        this.isSaving = false;
      },
      error: err => {
        console.error('Failed to create overtime rule', err);
        this.error = err.error?.message || 'Failed to create overtime rule.';
        this.isSaving = false;
      },
    });
  }

  deleteRule(rule: OTRule): void {
    if (!rule.id) {
      return;
    }

    this.overtimeConfigApi.deleteRule(rule.id).subscribe({
      next: () => {
        this.rules = this.rules.filter(existing => existing.id !== rule.id);
        this.selectedRule = this.rules[0] ?? null;
        this.successMessage = 'Overtime rule removed successfully.';
      },
      error: err => {
        console.error('Failed to delete overtime rule', err);
        this.error = err.error?.message || 'Failed to delete overtime rule.';
      },
    });
  }
}
