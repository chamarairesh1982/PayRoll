export type ApprovalStatus = 'Pending' | 'Approved' | 'Rejected' | 'Returned';

export type ApprovalAction = 'Approve' | 'Reject' | 'Send Back' | 'Comment';

export type ApprovalRequestType =
  | 'Leave Request'
  | 'Overtime'
  | 'Loan'
  | 'Payroll Adjustment'
  | 'Employee Master Data Change';

export interface ApprovalHistoryEntry {
  actor: string;
  action: ApprovalAction;
  date: string;
  comment?: string;
  level?: string;
  status?: ApprovalStatus;
}

export interface ApprovalRequest {
  id: string;
  type: ApprovalRequestType;
  status: ApprovalStatus;
  requestedBy: string;
  requestedDate: string;
  employee: string;
  amount?: number;
  currency?: string;
  currentLevel?: string;
  payload: Record<string, unknown>;
  history: ApprovalHistoryEntry[];
}

export interface ApprovalRouteConfig {
  id: string;
  type: ApprovalRequestType;
  levels: string[];
  approvers: string[];
  threshold?: number | null;
}

export interface ApprovalSubmitPayload {
  type: ApprovalRequestType;
  employee: string;
  amount?: number;
  currency?: string;
  payload: Record<string, unknown>;
}
