import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { CrawlSource } from '@/types/crawler';
import { mockCrawlSources } from '@/utils/mockData';

export interface CrawlerLog {
  id: string;
  timestamp: string;
  source: string;
  type: 'info' | 'success' | 'warning' | 'error';
  message: string;
}

export const useCrawlerStore = defineStore('crawler', () => {
  const sources = ref<CrawlSource[]>(mockCrawlSources);
  const logs = ref<CrawlerLog[]>([
    {
      id: 'log-1',
      timestamp: new Date(Date.now() - 3600000).toISOString(),
      source: 'Batdongsan',
      type: 'info',
      message: 'Khởi động Crawler Worker Node #1...'
    },
    {
      id: 'log-2',
      timestamp: new Date(Date.now() - 3500000).toISOString(),
      source: 'Batdongsan',
      type: 'success',
      message: 'Đã thiết lập kết nối proxy an toàn. Mở cổng cào dữ liệu.'
    },
    {
      id: 'log-3',
      timestamp: new Date(Date.now() - 3000000).toISOString(),
      source: 'Nhadat24h',
      type: 'info',
      message: 'Đang trích xuất liên kết trang mục lục Quận 2...'
    },
    {
      id: 'log-4',
      timestamp: new Date(Date.now() - 2500000).toISOString(),
      source: 'Chotot BĐS',
      type: 'warning',
      message: 'Cảnh báo: Tốc độ phản hồi từ Chotot giảm đột ngột (HTTP 429). Đang điều chỉnh giãn cách.'
    }
  ]);

  const activeCrawlingId = ref<string | null>(null);

  const addSource = (source: CrawlSource) => {
    sources.value.push(source);
  };

  const updateSource = (updated: CrawlSource) => {
    const idx = sources.value.findIndex(s => s.id === updated.id);
    if (idx !== -1) {
      sources.value[idx] = updated;
    }
  };

  const deleteSource = (id: string) => {
    sources.value = sources.value.filter(s => s.id !== id);
  };

  const pushLog = (source: string, type: 'info' | 'success' | 'warning' | 'error', message: string) => {
    logs.value.unshift({
      id: `log-${Date.now()}-${Math.random()}`,
      timestamp: new Date().toISOString(),
      source,
      type,
      message
    });
    // Cap logs at 100 entries
    if (logs.value.length > 100) {
      logs.value.pop();
    }
  };

  return {
    sources,
    logs,
    activeCrawlingId,
    addSource,
    updateSource,
    deleteSource,
    pushLog
  };
});
