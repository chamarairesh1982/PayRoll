import { NgModule } from '@angular/core';

import { TableModule } from 'primeng/table';
import { PaginatorModule } from 'primeng/paginator';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { MenuModule } from 'primeng/menu';
import { RippleModule } from 'primeng/ripple';
import { CheckboxModule } from 'primeng/checkbox';
import { SkeletonModule } from 'primeng/skeleton';

const PRIMENG_MODULES = [
  TableModule,
  PaginatorModule,
  ButtonModule,
  InputTextModule,
  DropdownModule,
  MultiSelectModule,
  CalendarModule,
  TagModule,
  TooltipModule,
  ToastModule,
  ConfirmDialogModule,
  DialogModule,
  MenuModule,
  RippleModule,
  CheckboxModule,
  SkeletonModule,
];

@NgModule({
  imports: [...PRIMENG_MODULES],
  exports: [...PRIMENG_MODULES],
})
export class PrimeNgModule {}
