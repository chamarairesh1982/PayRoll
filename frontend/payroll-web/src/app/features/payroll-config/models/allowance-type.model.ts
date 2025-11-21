export type CalculationBasis =
  | 'FixedAmount'
  | 'PercentageOfBasic'
  | 'PercentageOfGross'
  | 'PerDay'
  | 'PerHour';

export interface AllowanceType {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  basis: CalculationBasis;
  isEpfApplicable: boolean;
  isEtfApplicable: boolean;
  isTaxable: boolean;
  isActive: boolean;
}
