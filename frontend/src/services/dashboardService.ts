import type { DashboardMetric, MarketPoint } from '@/types/dashboard';

import { api } from './api';

export const dashboardService = {
  async getMetrics(): Promise<DashboardMetric[]> {
    const { data } = await api.get('/dashboard/metrics');
    return data.data;
  },

  async getMarketTrend(): Promise<MarketPoint[]> {
    const { data } = await api.get('/dashboard/market-trend');
    return data.data;
  }
};
