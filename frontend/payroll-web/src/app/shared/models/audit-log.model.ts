export interface AuditLogEntry {
  id: string;
  entityName: string;
  entityId: string;
  action: string;
  beforeSnapshot: string;
  afterSnapshot: string;
  createdAt: string;
  createdBy: string;
}
