import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import {
  ApprovalAction,
  ApprovalHistoryEntry,
  ApprovalRequest,
  ApprovalRouteConfig,
  ApprovalSubmitPayload,
} from '../models/approval.models';

const MOCK_APPROVALS: ApprovalRequest[] = [
  {
    id: 'APR-1001',
    type: 'Leave Request',
    status: 'Pending',
    requestedBy: 'kasun.perera',
    requestedDate: '2024-11-12T09:30:00Z',
    employee: 'EMP-1023 / Dilani Silva',
    currentLevel: 'Level 1 Manager',
    payload: {
      leaveType: 'Annual',
      startDate: '2024-11-15',
      endDate: '2024-11-18',
      totalDays: 3,
      reason: 'Family commitment',
    },
    history: [
      {
        actor: 'system',
        action: 'Comment',
        date: '2024-11-12T09:30:00Z',
        comment: 'Submitted for approval.',
        status: 'Pending',
      },
    ],
  },
  {
    id: 'APR-1002',
    type: 'Overtime',
    status: 'Pending',
    requestedBy: 'amila.jayasinghe',
    requestedDate: '2024-11-10T12:10:00Z',
    employee: 'EMP-0874 / Rashmi Fernando',
    amount: 14500,
    currency: 'LKR',
    currentLevel: 'Level 2 HR',
    payload: {
      workDate: '2024-11-08',
      hours: 6,
      rate: 2416.67,
      comment: 'Project deployment support',
    },
    history: [
      {
        actor: 'level1.manager',
        action: 'Approve',
        date: '2024-11-10T13:00:00Z',
        comment: 'Approved by line manager.',
        status: 'Pending',
        level: 'Level 1 Manager',
      },
    ],
  },
  {
    id: 'APR-1003',
    type: 'Loan',
    status: 'Pending',
    requestedBy: 'nuwan.dissanayake',
    requestedDate: '2024-11-05T08:20:00Z',
    employee: 'EMP-0541 / Ishara Perera',
    amount: 65000,
    currency: 'LKR',
    currentLevel: 'Finance',
    payload: {
      loanType: 'Salary advance',
      amount: 65000,
      repaymentMonths: 6,
      reason: 'Medical expenses',
    },
    history: [
      {
        actor: 'system',
        action: 'Comment',
        date: '2024-11-05T08:20:00Z',
        comment: 'Awaiting finance approval for amounts above LKR 50,000.',
        status: 'Pending',
      },
    ],
  },
  {
    id: 'APR-1004',
    type: 'Payroll Adjustment',
    status: 'Approved',
    requestedBy: 'payroll.admin',
    requestedDate: '2024-10-28T10:15:00Z',
    employee: 'EMP-0669 / Malith Wijesinghe',
    amount: 12000,
    currency: 'LKR',
    currentLevel: 'Finance',
    payload: {
      adjustmentType: 'Manual Earnings',
      description: 'Special project bonus',
      amount: 12000,
    },
    history: [
      {
        actor: 'level1.manager',
        action: 'Approve',
        date: '2024-10-28T11:00:00Z',
        comment: 'OK to proceed.',
        status: 'Pending',
        level: 'Level 1 Manager',
      },
      {
        actor: 'finance.head',
        action: 'Approve',
        date: '2024-10-29T09:45:00Z',
        comment: 'Budget confirmed.',
        status: 'Approved',
        level: 'Finance',
      },
    ],
  },
  {
    id: 'APR-1005',
    type: 'Employee Master Data Change',
    status: 'Returned',
    requestedBy: 'hr.executive',
    requestedDate: '2024-11-01T07:00:00Z',
    employee: 'EMP-0330 / Sheran Rodrigo',
    currentLevel: 'Level 2 HR',
    payload: {
      changeType: 'Bank Account Update',
      oldValue: 'BOC 10200123',
      newValue: 'HNB 44556677',
    },
    history: [
      {
        actor: 'level1.manager',
        action: 'Send Back',
        date: '2024-11-02T08:05:00Z',
        comment: 'Please attach bank confirmation letter.',
        status: 'Returned',
        level: 'Level 1 Manager',
      },
    ],
  },
];

const MOCK_CONFIGS: ApprovalRouteConfig[] = [
  {
    id: 'CFG-001',
    type: 'Leave Request',
    levels: ['Level 1 Manager', 'Level 2 HR'],
    approvers: ['Manager', 'HR'],
    threshold: null,
  },
  {
    id: 'CFG-002',
    type: 'Overtime',
    levels: ['Level 1 Manager', 'Level 2 HR'],
    approvers: ['Manager', 'HR'],
    threshold: 30000,
  },
  {
    id: 'CFG-003',
    type: 'Loan',
    levels: ['Level 1 Manager', 'Finance'],
    approvers: ['Manager', 'Finance'],
    threshold: 50000,
  },
  {
    id: 'CFG-004',
    type: 'Payroll Adjustment',
    levels: ['Level 1 Manager', 'Finance'],
    approvers: ['Manager', 'Finance'],
    threshold: 50000,
  },
];

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private approvalsSubject = new BehaviorSubject<ApprovalRequest[]>(MOCK_APPROVALS);
  private configsSubject = new BehaviorSubject<ApprovalRouteConfig[]>(MOCK_CONFIGS);

  constructor(private authService: AuthService) {}

  getInboxItems(): Observable<ApprovalRequest[]> {
    return this.approvalsSubject.asObservable();
  }

  getConfigs(): Observable<ApprovalRouteConfig[]> {
    return this.configsSubject.asObservable();
  }

  submitRequest(payload: ApprovalSubmitPayload): void {
    const now = new Date().toISOString();
    const username = this.authService.getUserName() || 'current.user';
    const newRequest: ApprovalRequest = {
      id: `APR-${Math.floor(1000 + Math.random() * 9000)}`,
      type: payload.type,
      status: 'Pending',
      requestedBy: username,
      requestedDate: now,
      employee: payload.employee,
      amount: payload.amount,
      currency: payload.currency,
      currentLevel: 'Level 1 Manager',
      payload: payload.payload,
      history: [
        {
          actor: username,
          action: 'Comment',
          date: now,
          comment: 'Submitted for approval.',
          status: 'Pending',
          level: 'Level 1 Manager',
        },
      ],
    };

    this.approvalsSubject.next([newRequest, ...this.approvalsSubject.value]);
  }

  performAction(requestId: string, action: ApprovalAction, comment?: string): void {
    const username = this.authService.getUserName() || 'current.user';
    const now = new Date().toISOString();

    const updated = this.approvalsSubject.value.map(request => {
      if (request.id !== requestId) {
        return request;
      }

      const nextStatus = this.getNextStatus(request.status, action);
      const historyEntry: ApprovalHistoryEntry = {
        actor: username,
        action,
        date: now,
        comment,
        status: nextStatus ?? request.status,
        level: request.currentLevel,
      };

      return {
        ...request,
        status: nextStatus ?? request.status,
        history: [historyEntry, ...request.history],
      };
    });

    this.approvalsSubject.next(updated);
  }

  saveConfig(config: ApprovalRouteConfig): void {
    const configs = this.configsSubject.value;
    const existingIndex = configs.findIndex(item => item.id === config.id);
    const updatedConfig = {
      ...config,
      id: config.id || `CFG-${Math.floor(100 + Math.random() * 900)}`,
    };

    const updatedConfigs =
      existingIndex === -1
        ? [updatedConfig, ...configs]
        : configs.map(item => (item.id === config.id ? updatedConfig : item));

    this.configsSubject.next(updatedConfigs);
  }

  private getNextStatus(currentStatus: string, action: ApprovalAction): ApprovalRequest['status'] | null {
    if (action === 'Approve') {
      return 'Approved';
    }
    if (action === 'Reject') {
      return 'Rejected';
    }
    if (action === 'Send Back') {
      return 'Returned';
    }

    return null;
  }
}
