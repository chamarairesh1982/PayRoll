export interface TaxSlab {
  from: number;
  to: number | null;
  rate: number;
}

export type TaxReliefType = 'IncomeRelief' | 'TaxRebate';
export type TaxReliefFrequency = 'Monthly' | 'Annual';

export interface TaxRelief {
  id: string;
  description: string;
  amount: number;
  reliefType: TaxReliefType;
  frequency: TaxReliefFrequency;
}

export interface TaxScheme {
  id: string;
  name: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  slabs: TaxSlab[];
  reliefs: TaxRelief[];
  isActive: boolean;
}
