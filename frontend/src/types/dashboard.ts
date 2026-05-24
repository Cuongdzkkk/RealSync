export interface DashboardMetric {
  label: string;
  value: number;
  suffix?: string;
  trend: string;
  variant: 'success' | 'warning' | 'info' | 'danger';
}

export interface MarketPoint {
  label: string;
  value: number;
}
