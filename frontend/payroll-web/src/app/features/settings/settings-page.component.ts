import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, CardModule, TagModule],
  templateUrl: './settings-page.component.html',
  styleUrls: ['./settings-page.component.scss'],
})
export class SettingsPageComponent {
  settingType = this.route.snapshot.data['settingType'] ?? 'Settings';

  constructor(private route: ActivatedRoute) {}
}
