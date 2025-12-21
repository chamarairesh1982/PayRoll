import { TaxCalculationResult } from '../services/tax-calculation.service';

export interface TaxPreviewRow extends TaxCalculationResult {
  employeeId: string;
  employeeName: string;
  taxCategory: string;
}
