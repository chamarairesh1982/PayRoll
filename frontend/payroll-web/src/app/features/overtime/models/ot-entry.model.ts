export type OvertimeType = 'Normal' | 'Weekend' | 'Holiday';

export type OvertimeStatus = 'Draft' | 'Submitted' | 'Approved' | 'Rejected';

export interface OTEntry {
  id: string;

  employeeId: string;
  employeeCode?: string;
  employeeName?: string;

  workDate: string;

  rawMinutes: number;

  type: OvertimeType;

  status: OvertimeStatus;

  comment?: string | null;

  approvedByUserId?: string | null;
  approvedAtUtc?: string | null;

  createdByUserId?: string | null;
  createdAtUtc: string;
  updatedAtUtc?: string | null;

  isLockedForPayroll: boolean;
}
