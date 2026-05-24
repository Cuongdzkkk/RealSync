<script setup lang="ts">
import { computed } from 'vue';
import type { Property } from '@/types/property';
import { formatCurrency } from '@/utils/format';
import SimpleBarChart from '../charts/SimpleBarChart.vue';

const props = defineProps<{
  property: Property;
}>();

const badgeLabel = computed(() => {
  const map: Record<string, string> = {
    published: 'Đang bán',
    verified: 'Đã xác thực',
    draft: 'Nháp',
    expired: 'Hết hạn'
  };
  return map[props.property.status] ?? props.property.status;
});

// Simple mock analytics for the mini chart
const analyticsData = [
  { label: 'T2', value: 12 },
  { label: 'T3', value: 18 },
  { label: 'T4', value: 14 },
  { label: 'T5', value: 24 }
];
</script>

<template>
  <div class="property-row">
    <div class="property-row__thumb">
      <img :src="property.imageUrl" :alt="property.title" />
      <div class="property-row__badge">{{ badgeLabel }}</div>
    </div>
    
    <div class="property-row__info">
      <div class="property-row__price numeric">{{ formatCurrency(property.price) }}</div>
      <div class="property-row__specs">
        <span>{{ property.bedrooms }} phòng ngủ</span>
        <span>·</span>
        <!-- Optional chaining fallback for bathrooms if missing from mock -->
        <span>{{ (property as any).bathrooms ?? 2 }} phòng tắm</span>
        <span>·</span>
        <span>{{ property.acreage }}m²</span>
      </div>
      <div class="property-row__address">{{ property.address }}</div>
    </div>

    <div class="property-row__analytics">
      <div class="property-row__analytics-chart">
        <SimpleBarChart :data="analyticsData" :hide-labels="true" />
      </div>
      <div class="property-row__analytics-views">
        {{ (property as any).viewCount ?? Math.floor(Math.random() * 500) + 100 }} views
      </div>
    </div>
  </div>
</template>

<style scoped>
.property-row {
  display: grid;
  grid-template-columns: 88px 1fr 80px;
  gap: 12px;
  padding: 12px 8px;
  border-bottom: 1px solid var(--color-divider);
  transition: background var(--duration-fast) var(--ease-standard),
              border-radius var(--duration-fast) var(--ease-standard);
  cursor: pointer;
}

.property-row:hover {
  background: var(--color-surface-hover);
  border-radius: 6px;
  border-bottom-color: transparent;
}

.property-row__thumb {
  position: relative;
  width: 88px;
  height: 64px;
  border-radius: 8px;
  overflow: hidden;
  background: var(--color-border);
}

.property-row__thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.property-row__badge {
  position: absolute;
  bottom: 6px;
  left: 6px;
  background: rgba(13, 13, 13, 0.75);
  color: #FFFFFF;
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: 4px;
  line-height: 1.2;
}

.property-row__info {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 2px 0;
  min-width: 0;
}

.property-row__price {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.2;
}

.property-row__specs {
  font-size: 12px;
  font-weight: 500;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
}

.property-row__address {
  font-size: 12px;
  font-weight: 400;
  color: var(--color-text-secondary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.property-row__analytics {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  justify-content: space-between;
  padding: 2px 0;
}

.property-row__analytics-chart {
  width: 100%;
  padding-top: 4px;
}

.property-row__analytics-views {
  font-size: 11px;
  color: var(--color-text-muted);
}
</style>
