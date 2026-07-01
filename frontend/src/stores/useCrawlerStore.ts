import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { CrawlSource } from '@/types/crawler';
import { api } from '@/services/api';

export interface CrawlerLog {
  id: string;
  timestamp: string;
  source: string;
  type: 'info' | 'success' | 'warning' | 'error';
  message: string;
}

export interface CrawlRunResult {
  jobId: string;
  propertyId: string;
  propertyTitle: string;
  address: string;
  price: number;
  aiScore: number;
  totalCreated?: number;
}

export const useCrawlerStore = defineStore('crawler', () => {
  const sources = ref<CrawlSource[]>([]);
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

  const loading = ref(false);

  async function fetchSources() {
    loading.value = true;
    try {
      const { data: res } = await api.get('/crawlers/sources');
      sources.value = res.data ?? [];
    } catch (err: any) {
      pushLog('System', 'error', err?.response?.data?.message || err?.message || 'Không tải được danh sách nguồn cào');
    } finally {
      loading.value = false;
    }
  }

  async function runCrawler(
    sourceId: string,
    area: string,
    province: string,
    propertyType: string,
    category: string,
    enableAiFilter: boolean,
    crawlMode: string = 'Property',
    prompt: string = '',
    useLocationFilter: boolean = true
  ): Promise<CrawlRunResult> {
    const { data: res } = await api.post(`/crawlers/sources/${sourceId}/run`, {
      area,
      province,
      propertyType,
      category,
      enableAiFilter,
      crawlMode,
      prompt,
      useLocationFilter
    });

    return res.data;
  }

  async function addSource(source: Omit<CrawlSource, 'id' | 'lastRunAt'>) {
    const { data: res } = await api.post('/crawlers/sources', source);
    sources.value.push(res.data);
    return res.data as CrawlSource;
  }

  async function updateSource(updated: CrawlSource) {
    const { data: res } = await api.put(`/crawlers/sources/${updated.id}`, updated);
    const idx = sources.value.findIndex(s => s.id === updated.id);
    if (idx !== -1) {
      sources.value[idx] = res.data;
    }
    return res.data as CrawlSource;
  }

  async function deleteSource(id: string) {
    await api.delete(`/crawlers/sources/${id}`);
    sources.value = sources.value.filter(s => s.id !== id);
  }

  function pushLog(source: string, type: CrawlerLog['type'], message: string) {
    logs.value.unshift({
      id: `log-${Date.now()}-${Math.random()}`,
      timestamp: new Date().toISOString(),
      source,
      type,
      message
    });

    if (logs.value.length > 100) {
      logs.value.pop();
    }
  }

  return {
    sources,
    logs,
    loading,
    fetchSources,
    addSource,
    updateSource,
    deleteSource,
    runCrawler,
    pushLog
  };
});
