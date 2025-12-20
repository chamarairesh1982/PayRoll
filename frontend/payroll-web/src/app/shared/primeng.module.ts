import { NgModule } from '@angular/core';
import { TableModule } from 'primeng/table';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputMaskModule } from 'primeng/inputmask';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

const PRIMENG_MODULES = [
  TableModule,
  DropdownModule,
  CalendarModule,
  InputTextModule,
  InputNumberModule,
  InputMaskModule,
  ButtonModule,
  CheckboxModule,
  ToastModule,
  ConfirmDialogModule,
  DialogModule,
  TooltipModule,
  ProgressSpinnerModule,
];

@NgModule({
  imports: PRIMENG_MODULES,
  exports: PRIMENG_MODULES,
})
export class PrimeNgModule {}
