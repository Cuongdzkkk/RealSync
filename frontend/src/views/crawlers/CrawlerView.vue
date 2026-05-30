<script setup lang="ts">
import { computed } from 'vue';
import PageHeader from '@/components/common/PageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockCrawlSources } from '@/utils/mockData';
import { formatDate } from '@/utils/format';

const totalSources = computed(() => mockCrawlSources.length);
const activeSources = computed(() => mockCrawlSources.filter(s => s.isActive).length);
const avgSuccess = computed(() => Math.round(mockCrawlSources.reduce((a, s) => a + s.successRate, 0) / mockCrawlSources.length));
const todayListings = computed(() => mockCrawlSources.reduce((a, s) => a + s.listingsToday, 0));
</script>

<template>
  <section class="page">
    <PageHeader title="Máy thu thập" description="Quản lý nguồn crawl, trạng thái job và chất lượng dữ liệu." />

    <div class="crawler-metrics">
      <div class="crawler-metric" style="background: var(--color-pastel-blue);">
        <span class="crawler-metric__label">Tổng nguồn</span>
        <strong class="crawler-metric__value">{{ totalSources }}</strong>
      </div>
      <div class="crawler-metric" style="background: var(--color-pastel-green);">
        <span class="crawler-metric__label">Đang hoạt động</span>
        <strong class="crawler-metric__value">{{ activeSources }}</strong>
      </div>
      <div class="crawler-metric" style="background: var(--color-pastel-pink);">
        <span class="crawler-metric__label">Tỉ lệ thành công</span>
        <strong class="crawler-metric__value numeric">{{ avgSuccess }}%</strong>
      </div>
      <div class="crawler-metric" style="background: var(--color-pastel-peach);">
        <span class="crawler-metric__label">Listing hôm nay</span>
        <strong class="crawler-metric__value numeric">{{ todayListings }}</strong>
      </div>
    </div>

    <section class="panel">
      <div class="panel__body" style="padding: 0;">
        <table class="crawler-table">
          <thead>
            <tr>
              <th>Nguồn</th>
              <th>URL</th>
              <th>Trạng thái</th>
              <th>Tỉ lệ</th>
              <th>Listing</th>
              <th>Lần chạy cuối</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="source in mockCrawlSources" :key="source.id">
              <td class="crawler-table__name">{{ source.name }}</td>
              <td><span class="mono">{{ source.baseUrl }}</span></td>
              <td>
                <StatusBadge
                  :label="source.isActive ? 'Hoạt động' : 'Tạm dừng'"
                  :variant="source.isActive ? 'success' : 'muted'"
                  :pulse="source.isActive"
                />
              </td>
              <td class="numeric">{{ source.successRate }}%</td>
              <td class="numeric">{{ source.listingsToday }}</td>
              <td class="crawler-table__date">{{ formatDate(source.lastRunAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </section>
</template>

<style scoped>
.crawler-metrics {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

.crawler-metric {
  border-radius: 12px;
  padding: 16px 20px;
}

.crawler-metric__label {
  display: block;
  font-size: 10px;
  font-weight: 500;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--color-text-secondary);
  margin-bottom: 8px;
}

.crawler-metric__value {
  display: block;
  font-size: 28px;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
  line-height: 1;
}

.crawler-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.crawler-table th {
  font-size: 10px;
  font-weight: 500;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--color-text-muted);
  text-align: left;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border-strong);
  white-space: nowrap;
}

.crawler-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-divider);
  color: var(--color-text-primary);
}

.crawler-table tbody tr:hover {
  background: var(--color-surface-hover);
}

.crawler-table tbody tr:last-child td {
  border-bottom: none;
}

.crawler-table__name {
  font-weight: 600;
}

.crawler-table__date {
  color: var(--color-text-muted);
  font-size: 12px;
}

.mono {
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--color-text-secondary);
}

@media (max-width: 640px) {
  .crawler-metrics {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
