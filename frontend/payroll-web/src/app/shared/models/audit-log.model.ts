export interface AuditLogEntry {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  beforeJson?: string | null;
  afterJson?: string | null;
  timestampUtc: string;
  actorUserId: string;
  actorDisplayName?: string | null;
  correlationId?: string | null;
}
