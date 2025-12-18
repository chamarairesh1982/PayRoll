export interface CompanyOption {
  id: string;
  code: string;
  name: string;
}

export interface BranchOption {
  id: string;
  code: string;
  name: string;
  companyId: string;
}

export interface CostCenterOption {
  id: string;
  code: string;
  name: string;
  companyId?: string | null;
  branchId?: string | null;
}
