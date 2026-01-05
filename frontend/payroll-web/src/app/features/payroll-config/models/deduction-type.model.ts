import { CalculationBasis } from './allowance-type.model';

export interface DeductionType {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  basis: CalculationBasis;
  isPreTax: boolean;
  isPostTax: boolean;
  isActive: boolean;
}
