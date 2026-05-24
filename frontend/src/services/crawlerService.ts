import type { CrawlSource } from '@/types/crawler';

import { api } from './api';

export const crawlerService = {
  async getSources(): Promise<CrawlSource[]> {
    const { data } = await api.get('/crawl-sources');
    return data.data;
  }
};
