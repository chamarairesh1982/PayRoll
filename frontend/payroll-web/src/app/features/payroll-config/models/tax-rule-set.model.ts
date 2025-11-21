export interface TaxSlab {
  id?: string;
  fromAmount: number;
  toAmount?: number | null;
  ratePercent: number;
  order: number;
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
}
