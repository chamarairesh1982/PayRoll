export interface EpfEtfRuleSet {
  id: string;
  name: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  employeeEpfRate: number;
  employerEpfRate: number;
  employerEtfRate: number;
  minimumWageForEpf?: number | null;
  maximumEarningForEpf?: number | null;
  maximumEarningForEtf?: number | null;
  isDefault: boolean;
  isActive: boolean;
}
