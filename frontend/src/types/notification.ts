export interface NotificationItem {
  id: string;
  title: string;
  description: string;
  tone: 'success' | 'warning' | 'danger' | 'info' | 'ai';
  createdAt: string;
}
