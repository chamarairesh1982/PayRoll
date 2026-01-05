export type TaxCategory = 'Resident' | 'NonResident' | 'SpecialRate';

export type TaxExemptionKey = 'PrimaryEmployment' | 'SeniorCitizen' | 'Disabled' | 'ForeignIncome';

export interface TaxExemptions {
  PrimaryEmployment: boolean;
  SeniorCitizen: boolean;
  Disabled: boolean;
  ForeignIncome: boolean;
}

export interface EmployeeTaxProfile {
  id: string;
  employeeId: string;
  taxCategory: TaxCategory;
  isResident: boolean;
  exemptions: TaxExemptions;
  lastUpdated: string;
}
