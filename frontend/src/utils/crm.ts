import type { LeadActivityType, LeadPriority, LeadStatus, LeadTemperature } from '@/types/crm/lead';

export type FollowUpVisualState = 'overdue' | 'today' | 'upcoming' | 'none' | 'inactive';

export const leadStatusLabels: Record<LeadStatus, string> = {
  New: 'Mới',
  Contacted: 'Đã liên hệ',
  Qualified: 'Đủ điều kiện',
  Proposal: 'Đang đề xuất',
  Won: 'Thành công',
  Lost: 'Thất bại'
};

export const leadPriorityLabels: Record<LeadPriority, string> = {
  Low: 'Thấp',
  Normal: 'Bình thường',
  High: 'Cao',
  Urgent: 'Khẩn cấp'
};

export const activityTypeLabels: Record<LeadActivityType, string> = {
  Call: 'Cuộc gọi',
  Email: 'Email',
  Meeting: 'Cuộc hẹn',
  Note: 'Ghi chú',
  StatusChange: 'Đổi trạng thái',
  Assigned: 'Phân công',
  FollowUp: 'Đặt lịch chăm sóc',
  Converted: 'Chuyển thành khách hàng'
};

export function getLeadTemperature(score: number): LeadTemperature {
  if (score >= 70) return 'Hot';
  if (score >= 40) return 'Warm';
  return 'Cold';
}

export function getLeadStatusLabel(status: LeadStatus): string {
  return leadStatusLabels[status];
}

export function getLeadPriorityLabel(priority: LeadPriority): string {
  return leadPriorityLabels[priority];
}

export function getActivityTypeLabel(type: LeadActivityType): string {
  return activityTypeLabels[type];
}

export function getFollowUpState(date?: string | null, status?: LeadStatus): FollowUpVisualState {
  if (status === 'Won' || status === 'Lost') return date ? 'inactive' : 'none';
  if (!date) return 'none';

  const followUp = new Date(date);
  if (Number.isNaN(followUp.getTime())) return 'none';

  const now = new Date();
  const startToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const startTomorrow = new Date(startToday);
  startTomorrow.setDate(startToday.getDate() + 1);
  const nextWeek = new Date(now);
  nextWeek.setDate(now.getDate() + 7);

  if (followUp < now) return 'overdue';
  if (followUp >= startToday && followUp < startTomorrow) return 'today';
  if (followUp <= nextWeek) return 'upcoming';
  return 'upcoming';
}

export function formatRelativeDate(value?: string | null): string {
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

export function formatDateTime(value?: string | null): string {
  if (!value) return 'Chưa cập nhật';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Chưa cập nhật';
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date);
}

export function formatVnd(value?: number | null): string {
  if (value === null || value === undefined) return 'Chưa cập nhật';
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0
  }).format(value);
}

export function getInitials(name?: string | null): string {
  if (!name) return 'NA';
  return name
    .trim()
    .split(/\s+/)
    .slice(-2)
    .map((part) => part.charAt(0).toUpperCase())
    .join('');
}

export function formatNullableText(value?: string | number | null): string {
  if (value === null || value === undefined || value === '') return 'Chưa cập nhật';
  return String(value);
}
