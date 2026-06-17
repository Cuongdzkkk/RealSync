import { mockNotifications } from '@/mocks/crm/notifications';
import type { ApiResponse } from '@/types/crm/lead';
import type { CrmNotification, NotificationQuery, NotificationSummary } from '@/types/crm/notification';

const STORAGE_KEY = 'realsync.crm.mock.notifications';

function clone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value)) as T;
}

function delay() {
  const ms = 150 + Math.floor(Math.random() * 251);
  return new Promise((resolve) => window.setTimeout(resolve, ms));
}

function readDb(): CrmNotification[] {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockNotifications));
    return clone(mockNotifications);
  }

  try {
    return JSON.parse(raw) as CrmNotification[];
  } catch {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockNotifications));
    return clone(mockNotifications);
  }
}

function writeDb(items: CrmNotification[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
}

function ok<T>(data: T, message = 'Thành công'): ApiResponse<T> {
  return { success: true, statusCode: 200, message, data };
}

function findOwned(items: CrmNotification[], userId: string, id: string): CrmNotification {
  const item = items.find((notification) => notification.id === id && notification.userId === userId);
  if (!item) throw new Error('Không tìm thấy thông báo.');
  return item;
}

function applyQuery(items: CrmNotification[], userId: string, query: NotificationQuery): CrmNotification[] {
  let output = items.filter((item) => item.userId === userId);
  const keyword = query.search.trim().toLowerCase();

  if (keyword) {
    output = output.filter((item) =>
      [item.title, item.message, item.data]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(keyword))
    );
  }

  const readFilter = query.readFilter ?? 'all';
  if (readFilter === 'read') output = output.filter((item) => item.isRead);
  if (readFilter === 'unread') output = output.filter((item) => !item.isRead);
  if (query.isRead !== null && query.isRead !== undefined) output = output.filter((item) => item.isRead === query.isRead);
  if (query.type) output = output.filter((item) => item.type === query.type);
  if (query.fromDate) output = output.filter((item) => new Date(item.createdAt) >= new Date(query.fromDate!));
  if (query.toDate) output = output.filter((item) => new Date(item.createdAt) <= new Date(query.toDate!));

  const sortBy = query.sortBy ?? 'createdAt';
  const direction = query.sortDirection === 'asc' ? 1 : -1;
  output.sort((a, b) => String(a[sortBy] ?? '').localeCompare(String(b[sortBy] ?? ''), 'vi') * direction);

  return output;
}

function makeSummary(items: CrmNotification[], userId: string): NotificationSummary {
  const owned = items.filter((item) => item.userId === userId);
  const unreadCount = owned.filter((item) => !item.isRead).length;
  return {
    totalCount: owned.length,
    unreadCount,
    readCount: owned.length - unreadCount
  };
}

export const crmNotificationMockService = {
  resetNotificationMocks() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockNotifications));
  },

  async getNotifications(userId: string, query: NotificationQuery) {
    await delay();
    const filtered = applyQuery(readDb(), userId, query);
    const totalCount = filtered.length;
    const start = (query.page - 1) * query.pageSize;
    const data = filtered.slice(start, start + query.pageSize);

    return {
      ...ok(clone(data)),
      meta: {
        page: query.page,
        pageSize: query.pageSize,
        totalCount,
        totalPages: Math.max(1, Math.ceil(totalCount / query.pageSize))
      }
    };
  },

  async getNotificationById(userId: string, id: string) {
    await delay();
    return ok(clone(findOwned(readDb(), userId, id)));
  },

  async getUnreadCount(userId: string) {
    await delay();
    return ok(makeSummary(readDb(), userId).unreadCount);
  },

  async getSummary(userId: string) {
    await delay();
    return ok(makeSummary(readDb(), userId));
  },

  async markAsRead(userId: string, id: string) {
    await delay();
    const items = readDb();
    const item = findOwned(items, userId, id);
    if (!item.isRead) {
      item.isRead = true;
      item.readAt = new Date().toISOString();
      writeDb(items);
    }
    return ok(clone(item));
  },

  async markAllAsRead(userId: string) {
    await delay();
    const items = readDb();
    const now = new Date().toISOString();
    const updated = items.map((item) =>
      item.userId === userId && !item.isRead ? { ...item, isRead: true, readAt: now } : item
    );
    writeDb(updated);
    return ok(true);
  },

  async deleteNotification(userId: string, id: string) {
    await delay();
    const items = readDb();
    findOwned(items, userId, id);
    writeDb(items.filter((item) => !(item.userId === userId && item.id === id)));
    return ok(true, 'Xóa thành công');
  }
};
