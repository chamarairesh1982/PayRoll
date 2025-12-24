import { NgModule } from '@angular/core';

import { TableModule } from 'primeng/table';
import { PaginatorModule } from 'primeng/paginator';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { CalendarModule } from 'primeng/calendar';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { MenuModule } from 'primeng/menu';
import { RippleModule } from 'primeng/ripple';
import { CheckboxModule } from 'primeng/checkbox';
import { SkeletonModule } from 'primeng/skeleton';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ChipModule } from 'primeng/chip';
import { TimelineModule } from 'primeng/timeline';
import { TabMenuModule } from 'primeng/tabmenu';
import { TabViewModule } from 'primeng/tabview';
import { AvatarModule } from 'primeng/avatar';
import { DividerModule } from 'primeng/divider';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputSwitchModule } from 'primeng/inputswitch';

const PRIMENG_MODULES = [
  TableModule,
  PaginatorModule,
  ButtonModule,
  InputTextModule,
  DropdownModule,
  MultiSelectModule,
  CalendarModule,
  CardModule,
  TagModule,
  TooltipModule,
  ToastModule,
  ConfirmDialogModule,
  DialogModule,
  MenuModule,
  RippleModule,
  CheckboxModule,
  SkeletonModule,
  InputNumberModule,
  InputTextareaModule,
  ChipModule,
  TimelineModule,
  TabMenuModule,
  TabViewModule,
  AvatarModule,
  DividerModule,
  ProgressSpinnerModule,
  InputSwitchModule,
];

@NgModule({
  imports: [...PRIMENG_MODULES],
  exports: [...PRIMENG_MODULES],
})
export class PrimeNgModule { }
