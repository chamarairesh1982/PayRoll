export interface TaxSlab {
  id?: string;
  fromAmount: number;
  toAmount?: number | null;
  ratePercent: number;
  order: number;
}

export type TaxReliefType = 'IncomeRelief' | 'TaxRebate';
export type TaxReliefFrequency = 'Monthly' | 'Annual';

export interface TaxRelief {
  id?: string;
  name: string;
  amount: number;
  reliefType: TaxReliefType;
  frequency: TaxReliefFrequency;
}

export interface TaxRuleSet {
  id: string;
  name: string;
  yearOfAssessment: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isDefault: boolean;
  isActive: boolean;
  slabs: TaxSlab[];
  reliefs: TaxRelief[];
}
