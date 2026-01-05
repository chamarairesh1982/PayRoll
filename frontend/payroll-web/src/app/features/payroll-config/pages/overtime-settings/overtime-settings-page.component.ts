import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { OTRule, OTRulePayload } from '../../models/ot-rule.model';
import { OvertimeConfigApiService } from '../../services/overtime-config-api.service';

@Component({
  selector: 'app-overtime-settings-page',
  templateUrl: './overtime-settings-page.component.html',
  styleUrls: ['./overtime-settings-page.component.scss'],
  providers: [MessageService]
})
export class OvertimeSettingsPageComponent implements OnInit {
  rules: OTRule[] = [];
  selectedRule: OTRule | null = null;
  isLoading = true;
  isSaving = false;

  constructor(
    private overtimeConfigApi: OvertimeConfigApiService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadRules();
  }

  loadRules(): void {
    this.isLoading = true;
    this.overtimeConfigApi.getRules().subscribe({
      next: rules => {
        this.rules = rules;
        if (!this.selectedRule && rules.length) {
          this.selectedRule = rules[0];
        }
        this.isLoading = false;
      },
      error: err => {
        this.messageService.add({
          severity: 'error',
          summary: 'Retrieval Error',
          detail: 'Failed to synchronize institutional overtime protocols.'
        });
        this.isLoading = false;
      },
    });
  }

  selectRule(rule: OTRule): void {
    this.selectedRule = { ...rule };
  }

  createRule(): void {
    this.selectedRule = {
      id: '',
      type: 'Normal',
      multiplier: 1.5,
      roundToMinutes: 15,
      roundingMode: 'Nearest',
      dailyHoursCap: null,
      monthlyHoursCap: null,
      effectiveFrom: new Date().toISOString().split('T')[0],
      effectiveTo: null,
      isActive: true,
    };
  }

  saveRule(payload: OTRulePayload): void {
    this.isSaving = true;

    if (this.selectedRule?.id) {
      this.overtimeConfigApi.updateRule(this.selectedRule.id, payload).subscribe({
        next: rule => {
          this.rules = this.rules.map(existing => (existing.id === rule.id ? rule : existing));
          this.selectedRule = rule;
          this.messageService.add({
            severity: 'success',
            summary: 'Governance Updated',
            detail: 'Overtime parameters have been successfully persistence.'
          });
          this.isSaving = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Persistence Failure',
            detail: err.error?.message || 'Failed to modify overtime governance.'
          });
          this.isSaving = false;
        },
      });
      return;
    }

    this.overtimeConfigApi.createRule(payload).subscribe({
      next: rule => {
        this.rules = [...this.rules, rule];
        this.selectedRule = rule;
        this.messageService.add({
          severity: 'success',
          summary: 'Mandate Established',
          detail: 'New overtime governance policy has been initialized.'
        });
        this.isSaving = false;
      },
      error: err => {
        this.messageService.add({
          severity: 'error',
          summary: 'Initialization Failure',
          detail: err.error?.message || 'Failed to establish new overtime policy.'
        });
        this.isSaving = false;
      },
    });
  }

  deleteRule(rule: OTRule): void {
    if (!rule.id) return;

    this.overtimeConfigApi.deleteRule(rule.id).subscribe({
      next: () => {
        this.rules = this.rules.filter(existing => existing.id !== rule.id);
        this.selectedRule = this.rules[0] ?? null;
        this.messageService.add({
          severity: 'warn',
          summary: 'Policy Revoked',
          detail: 'Institutional overtime mandate has been decommissioned.'
        });
      },
      error: err => {
        this.messageService.add({
          severity: 'error',
          summary: 'Revocation Error',
          detail: err.error?.message || 'Failed to decommission overtime policy.'
        });
      },
    });
  }
}
