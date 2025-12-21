import { TaxScheme } from './tax-scheme.model';

export type TaxAuditAction = 'Created' | 'Updated' | 'Cloned' | 'Archived';

export interface TaxAuditLogEntry {
  id: string;
  schemeId: string;
  schemeName: string;
  action: TaxAuditAction;
  changedAt: string;
  changedBy: string;
  summary: string;
  before: Partial<TaxScheme> | null;
  after: Partial<TaxScheme> | null;
}
