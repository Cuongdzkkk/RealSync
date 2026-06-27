import { api } from './api';
import type { ActivityLog, ActivityLogApiResponse, ActivityLogQuery } from '@/types/crm/activity';

export const activityLogService = {
  async getActivityLogs(query: ActivityLogQuery): Promise<ActivityLogApiResponse<ActivityLog[]>> {
    const { data } = await api.get('/activity', {
      params: {
        ...query,
        userId: query.userId || undefined,
        entityType: query.entityType || undefined,
        entityId: query.entityId || undefined,
        action: query.action || undefined,
        fromDate: query.fromDate || undefined,
        toDate: query.toDate || undefined,
        search: query.search || undefined
      }
    });

    return data;
  }
};
