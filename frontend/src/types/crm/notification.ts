import type { ApiMeta, ApiResponse } from './lead';

export type NotificationType = 'System' | 'Lead' | 'Property' | 'Task' | 'Assignment';
export type NotificationReadFilter = 'all' | 'unread' | 'read';

export interface CrmNotification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  readAt?: string | null;
  data?: string | null;
  link?: string | null;
  createdAt: string;
}

export interface NotificationQuery {
  page: number;
  pageSize: number;
  search: string;
  isRead?: boolean | null;
  readFilter?: NotificationReadFilter;
  type?: NotificationType | null;
  fromDate?: string | null;
  toDate?: string | null;
  sortBy?: 'createdAt' | 'readAt' | 'type' | 'title';
  sortDirection?: 'asc' | 'desc';
}

export interface NotificationSummary {
  totalCount: number;
  unreadCount: number;
  readCount: number;
}

export interface NotificationData {
  eventType?: string;
  leadId?: string;
  customerId?: string;
  propertyId?: string;
  taskId?: string;
  assignedToId?: string;
  scheduledFor?: string;
  [key: string]: unknown;
}

export type NotificationApiResponse<T> = ApiResponse<T>;
export type NotificationApiMeta = ApiMeta;

export const NOTIFICATION_TYPES: NotificationType[] = ['System', 'Lead', 'Property', 'Task', 'Assignment'];
