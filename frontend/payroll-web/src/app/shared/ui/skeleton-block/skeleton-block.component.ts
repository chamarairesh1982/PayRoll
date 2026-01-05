import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-skeleton-block',
  standalone: true,
  imports: [CommonModule, SkeletonModule],
  templateUrl: './skeleton-block.component.html',
  styleUrls: ['./skeleton-block.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SkeletonBlockComponent {
  @Input() width = '100%';
  @Input() height = '1.25rem';
  @Input() borderRadius = '8px';
  @Input() count = 1;
}
