export interface TaxSlab {
  id?: string;
  fromAmount: number;
  toAmount?: number | null;
  rate: number;
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
  frequency?: string;
  slabs: TaxSlab[];
  reliefs: TaxRelief[];
}
