import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-error-banner',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  templateUrl: './error-banner.component.html',
  styleUrls: ['./error-banner.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorBannerComponent {
  @Input() title = 'Something went wrong';
  @Input() message = 'Please try again or contact support if the issue persists.';
  @Input() retryLabel = 'Retry';

  @Output() retry = new EventEmitter<void>();
}
