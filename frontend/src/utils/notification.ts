import type { UserProfile } from '@/types/auth';
import type { NotificationData, NotificationType } from '@/types/crm/notification';
import { mockCrmUsers } from '@/mocks/crm/users';

export type NotificationVariant = 'success' | 'warning' | 'danger' | 'info' | 'muted' | 'ai';

const typeLabels: Record<NotificationType, string> = {
  System: 'Hệ thống',
  Lead: 'Lead',
  Property: 'Bất động sản',
  Task: 'Công việc',
  Assignment: 'Phân công'
};

const typeVariants: Record<NotificationType, NotificationVariant> = {
  System: 'muted',
  Lead: 'ai',
  Property: 'success',
  Task: 'warning',
  Assignment: 'info'
};

const typeIcons: Record<NotificationType, string> = {
  System: 'Setting',
  Lead: 'User',
  Property: 'OfficeBuilding',
  Task: 'Clock',
  Assignment: 'UserFilled'
};

const eventLabels: Record<string, string> = {
  SystemRelease: 'Cập nhật hệ thống',
  LeadAssigned: 'Lead được phân công',
  LeadStatusChanged: 'Lead đổi trạng thái',
  FollowUpDue: 'Lịch chăm sóc',
  PropertyUpdated: 'Cập nhật BĐS',
  PropertyAssigned: 'Phân công BĐS',
  PropertyMatched: 'BĐS phù hợp',
  CustomerAssigned: 'Phân công khách hàng',
  TaskAssigned: 'Giao việc',
  TaskDue: 'Công việc đến hạn',
  CampaignReady: 'Chiến dịch',
  AnalyticsUpdated: 'Dữ liệu phân tích'
};

export function parseNotificationData(data?: string | null): NotificationData {
  if (!data) return {};
  try {
    const parsed: unknown = JSON.parse(data);
    return parsed && typeof parsed === 'object' && !Array.isArray(parsed) ? parsed as NotificationData : {};
  } catch {
    return {};
  }
}

function isSafeInternalPath(path: string): boolean {
  return (
    path === '/admin/notifications' ||
    path.startsWith('/admin/leads/') ||
    path.startsWith('/admin/customers/') ||
    path.startsWith('/admin/properties/') ||
    path === '/admin/leads' ||
    path === '/admin/customers' ||
    path === '/admin/properties' ||
    path === '/admin/dashboard' ||
    path === '/admin/content-ai' ||
    path === '/admin/crawlers' ||
    path === '/admin/settings'
  );
}

export function resolveNotificationLink(link?: string | null, parsedData: NotificationData = {}): string {
  if (link) {
    const trimmed = link.trim();
    const lower = trimmed.toLowerCase();

    if (!lower.startsWith('http:') && !lower.startsWith('https:') && !lower.startsWith('javascript:') && !lower.startsWith('data:')) {
      if (/^\/leads\/[^/]+$/.test(trimmed)) return `/admin${trimmed}`;
      if (/^\/customers\/[^/]+$/.test(trimmed)) return `/admin${trimmed}`;
      if (/^\/properties\/[^/]+$/.test(trimmed)) return `/admin${trimmed}`;
      if (trimmed.startsWith('/admin/') && isSafeInternalPath(trimmed)) return trimmed;
    }
  }

  if (typeof parsedData.leadId === 'string') return `/admin/leads/${parsedData.leadId}`;
  if (typeof parsedData.customerId === 'string') return `/admin/customers/${parsedData.customerId}`;
  if (typeof parsedData.propertyId === 'string') return `/admin/properties/${parsedData.propertyId}`;
  return '/admin/notifications';
}

export function getNotificationTypeLabel(type: NotificationType): string {
  return typeLabels[type];
}

export function getNotificationTypeVariant(type: NotificationType): NotificationVariant {
  return typeVariants[type];
}

export function getNotificationIcon(type: NotificationType): string {
  return typeIcons[type];
}

export function getNotificationEventLabel(data?: NotificationData): string {
  const eventType = typeof data?.eventType === 'string' ? data.eventType : '';
  return eventLabels[eventType] ?? eventType ?? 'Thông báo';
}

export function isFollowUpNotification(data?: NotificationData): boolean {
  return data?.eventType === 'FollowUpDue' && typeof data.scheduledFor === 'string';
}

export function isFollowUpOverdue(data?: NotificationData): boolean {
  if (!isFollowUpNotification(data)) return false;
  const date = new Date(String(data?.scheduledFor));
  return !Number.isNaN(date.getTime()) && date.getTime() < Date.now();
}

export function formatNotificationRelativeTime(value?: string | null): string {
  if (!value) return 'Chưa cập nhật';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Chưa cập nhật';

  const diff = date.getTime() - Date.now();
  const abs = Math.abs(diff);
  const minute = 60 * 1000;
  const hour = 60 * minute;
  const day = 24 * hour;

  if (abs < minute) return 'Vừa xong';
  if (abs < hour) return diff > 0 ? `Trong ${Math.round(abs / minute)} phút` : `${Math.round(abs / minute)} phút trước`;
  if (abs < day) return diff > 0 ? `Trong ${Math.round(abs / hour)} giờ` : `${Math.round(abs / hour)} giờ trước`;
  if (abs < 7 * day) return diff > 0 ? `Trong ${Math.round(abs / day)} ngày` : `${Math.round(abs / day)} ngày trước`;

  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date);
}

export function getNotificationUserId(user?: UserProfile | null): string {
  if (!user) return 'user-admin';
  const matchedUser = mockCrmUsers.find((item) => item.email === user.email);
  if (matchedUser) return matchedUser.id;

  const roleMap: Record<UserProfile['role'], string> = {
    Admin: 'user-admin',
    Manager: 'user-manager',
    Sales: 'user-sales-01',
    Agent: 'user-agent-01',
    Viewer: 'user-viewer',
    Marketing: 'user-marketing',
    'Data Analyst': 'user-data-analyst'
  };

  return roleMap[user.role] ?? 'user-admin';
}
