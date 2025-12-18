import { Component, OnInit } from '@angular/core';
import { OvertimeRuleConfig } from '../../models/overtime-rule-config.model';
import { OvertimeConfigApiService } from '../../services/overtime-config-api.service';

@Component({
  selector: 'app-overtime-settings-page',
  templateUrl: './overtime-settings-page.component.html',
  styleUrls: ['./overtime-settings-page.component.scss'],
})
export class OvertimeSettingsPageComponent implements OnInit {
  config: OvertimeRuleConfig | null = null;
  isLoading = true;
  isSaving = false;
  error: string | null = null;
  successMessage: string | null = null;

  constructor(private overtimeConfigApi: OvertimeConfigApiService) {}

  ngOnInit(): void {
    this.loadConfig();
  }

  loadConfig(): void {
    this.isLoading = true;
    this.error = null;

    this.overtimeConfigApi.getConfig().subscribe({
      next: config => {
        this.config = config;
        this.isLoading = false;
      },
      error: err => {
        console.error('Failed to load overtime rules', err);
        this.error = err.error?.message || 'Failed to load overtime configuration.';
        this.isLoading = false;
      },
    });
  }

  saveRules(updated: OvertimeRuleConfig): void {
    this.isSaving = true;
    this.successMessage = null;
    this.error = null;

    this.overtimeConfigApi.updateConfig(updated).subscribe({
      next: config => {
        this.config = config;
        this.successMessage = 'Overtime rules updated successfully.';
        this.isSaving = false;
      },
      error: err => {
        console.error('Failed to update overtime rules', err);
        this.error = err.error?.message || 'Failed to update overtime configuration.';
        this.isSaving = false;
      },
    });
  }
}
