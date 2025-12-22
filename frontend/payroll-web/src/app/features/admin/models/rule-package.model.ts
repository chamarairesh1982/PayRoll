export type RulePackageType = 'Tax' | 'Epf' | 'Etf' | 'Other';
export type RulePackageVersionStatus = 'Draft' | 'Active' | 'Retired';

export interface RulePackage {
  id: string;
  companyId: string;
  ruleType: RulePackageType;
  name: string;
  createdAt: string;
}

export interface RulePackageVersion {
  id: string;
  rulePackageId: string;
  versionNumber: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
  status: RulePackageVersionStatus;
  contentJson: string;
  contentHash: string;
  createdAt: string;
}
