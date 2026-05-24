export interface CrawlSource {
  id: string;
  name: string;
  baseUrl: string;
  isActive: boolean;
  successRate: number;
  lastRunAt: string;
  listingsToday: number;
}
