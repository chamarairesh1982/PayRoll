import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StatutoryReportsPageComponent } from './pages/statutory-reports-page/statutory-reports-page.component';

const routes: Routes = [
  { path: '', redirectTo: 'statutory', pathMatch: 'full' },
  { path: 'statutory', component: StatutoryReportsPageComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ReportsRoutingModule {}
