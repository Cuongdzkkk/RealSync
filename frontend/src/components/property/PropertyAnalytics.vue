<script setup lang="ts">
import { computed } from 'vue';

import type { Property } from '@/types/property';
import { formatCurrency, formatNumber } from '@/utils/format';

const props = withDefaults(defineProps<{
  properties: Property[];
  total?: number;
  loading?: boolean;
}>(), {
  total: undefined,
  loading: false
});

const numericArea = (property: Property) => Number(property.area ?? property.acreage ?? 0);
const isAvailable = (status: string) =>
  ['available', 'active', 'published', 'verified'].includes(status.toLowerCase());

const totalCount = computed(() => props.total ?? props.properties.length);
const availableCount = computed(() =>
  props.properties.filter((property) => isAvailable(String(property.status))).length
);
const averageArea = computed(() => {
  if (props.properties.length === 0) return 0;
  return props.properties.reduce((sum, property) => sum + numericArea(property), 0) / props.properties.length;
});
const averagePrice = computed(() => {
  if (props.properties.length === 0) return 0;
  return props.properties.reduce((sum, property) => sum + Number(property.price ?? 0), 0) / props.properties.length;
});

const categoryStats = computed(() => {
  const map = new Map<string, number>();
  props.properties.forEach((property) => {
    const label = property.categoryName ?? property.propertyCategoryName ?? 'Chưa phân loại';
    map.set(label, (map.get(label) ?? 0) + 1);
  });

  return Array.from(map.entries())
    .map(([label, value]) => ({ label, value }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 5);
});

const locationStats = computed(() => {
  const map = new Map<string, number>();
  props.properties.forEach((property) => {
    const label = property.provinceName ?? property.districtName ?? 'Chưa rõ khu vực';
    map.set(label, (map.get(label) ?? 0) + 1);
  });

  return Array.from(map.entries())
    .map(([label, value]) => ({ label, value }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 5);
});

const maxCategory = computed(() => Math.max(1, ...categoryStats.value.map((item) => item.value)));
const maxLocation = computed(() => Math.max(1, ...locationStats.value.map((item) => item.value)));
</script>

<template>
  <section class="property-analytics">
    <template v-if="loading">
      <div v-for="index in 4" :key="index" class="analytics-card skeleton-card">
        <span class="skeleton skeleton-title"></span>
        <span class="skeleton skeleton-value"></span>
        <span class="skeleton skeleton-line"></span>
      </div>
    </template>

    <template v-else>
      <article class="analytics-card">
        <span class="analytics-card__label">Tổng bất động sản</span>
        <strong class="analytics-card__value numeric">{{ formatNumber(totalCount) }}</strong>
        <span class="analytics-card__note">Theo bộ lọc hiện tại</span>
      </article>

      <article class="analytics-card">
        <span class="analytics-card__label">Available</span>
        <strong class="analytics-card__value numeric">{{ formatNumber(availableCount) }}</strong>
        <span class="analytics-card__note">Active / Available / Published</span>
      </article>

      <article class="analytics-card">
        <span class="analytics-card__label">Diện tích TB</span>
        <strong class="analytics-card__value numeric">{{ formatNumber(Math.round(averageArea)) }} m2</strong>
        <span class="analytics-card__note">Tính từ trang dữ liệu đang tải</span>
      </article>

      <article class="analytics-card">
        <span class="analytics-card__label">Giá TB</span>
        <strong class="analytics-card__value numeric">{{ formatCurrency(Math.round(averagePrice)) }}</strong>
        <span class="analytics-card__note">Tính từ trang dữ liệu đang tải</span>
      </article>

      <article class="analytics-panel">
        <div class="analytics-panel__header">
          <span>Theo danh mục</span>
        </div>
        <div v-if="categoryStats.length" class="analytics-bars">
          <div v-for="item in categoryStats" :key="item.label" class="analytics-bar">
            <span class="analytics-bar__label">{{ item.label }}</span>
            <span class="analytics-bar__track">
              <span class="analytics-bar__fill" :style="{ width: `${(item.value / maxCategory) * 100}%` }"></span>
            </span>
            <strong class="numeric">{{ item.value }}</strong>
          </div>
        </div>
        <div v-else class="analytics-panel__empty">Chưa có dữ liệu danh mục.</div>
      </article>

      <article class="analytics-panel">
        <div class="analytics-panel__header">
          <span>Theo khu vực</span>
        </div>
        <div v-if="locationStats.length" class="analytics-bars">
          <div v-for="item in locationStats" :key="item.label" class="analytics-bar">
            <span class="analytics-bar__label">{{ item.label }}</span>
            <span class="analytics-bar__track">
              <span class="analytics-bar__fill" :style="{ width: `${(item.value / maxLocation) * 100}%` }"></span>
            </span>
            <strong class="numeric">{{ item.value }}</strong>
          </div>
        </div>
        <div v-else class="analytics-panel__empty">Chưa có dữ liệu khu vực.</div>
      </article>
    </template>
  </section>
</template>

<style scoped>
.property-analytics {
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.analytics-card,
.analytics-panel {
  background: #ffffff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: var(--elevation-surface);
}

.analytics-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-height: 118px;
  padding: 16px;
}

.analytics-card__label,
.analytics-panel__header {
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.analytics-card__value {
  color: var(--color-text-primary);
  font-size: 22px;
  line-height: 1.1;
}

.analytics-card__note {
  color: var(--color-text-secondary);
  font-size: 12px;
  margin-top: auto;
}

.analytics-panel {
  display: flex;
  flex-direction: column;
  gap: 14px;
  grid-column: span 2;
  padding: 16px;
}

.analytics-bars {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.analytics-bar {
  align-items: center;
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(92px, 1fr) minmax(110px, 2fr) 28px;
}

.analytics-bar__label {
  color: var(--color-text-secondary);
  font-size: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.analytics-bar__track {
  background: #f1f5f9;
  border-radius: 999px;
  height: 8px;
  overflow: hidden;
}

.analytics-bar__fill {
  background: var(--color-yellow);
  border-radius: inherit;
  display: block;
  height: 100%;
}

.analytics-panel__empty {
  color: var(--color-text-muted);
  font-size: 12px;
  padding: 12px 0;
}

.skeleton-card {
  gap: 12px;
}

.skeleton-title {
  height: 12px;
  width: 50%;
}

.skeleton-value {
  height: 26px;
  width: 70%;
}

.skeleton-line {
  height: 12px;
  margin-top: auto;
  width: 86%;
}

@media (max-width: 1100px) {
  .property-analytics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .property-analytics {
    grid-template-columns: 1fr;
  }

  .analytics-panel {
    grid-column: span 1;
  }
}
</style>
