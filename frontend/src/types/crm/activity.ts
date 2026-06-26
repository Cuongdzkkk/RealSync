import type { ApiMeta, ApiResponse } from './lead';

export type ActivityAction = 'Login' | 'Create' | 'Update' | 'Delete' | 'StatusChange' | 'Assignment' | 'View' | 'Export';

export interface ActivityLog {
  id: string;
  userId?: string | null;
  userName?: string | null;
  userEmail?: string | null;
  userRole?: string | null;
  entityType: string;
  entityId?: string | null;
  action: ActivityAction;
  description?: string | null;
  oldValues?: string | null;
  newValues?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAt: string;
}

export interface ActivityLogQuery {
  page: number;
  pageSize: number;
  search: string;
  userId?: string | null;
  entityType?: string | null;
  entityId?: string | null;
  action?: ActivityAction | null;
  fromDate?: string | null;
  toDate?: string | null;
  sortBy?: 'createdAt' | 'action' | 'entityType' | 'userName';
  sortDirection?: 'asc' | 'desc';
}

export type ActivityLogApiResponse<T> = ApiResponse<T>;
export type ActivityLogApiMeta = ApiMeta;

export const ACTIVITY_ACTIONS: ActivityAction[] = [
  'Login',
  'Create',
  'Update',
  'Delete',
  'StatusChange',
  'Assignment',
  'View',
  'Export'
];
