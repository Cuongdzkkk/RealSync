<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

import PageHeader from '@/components/common/PageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockProperties } from '@/utils/mockData';
import { formatCurrency } from '@/utils/format';

const route = useRoute();
const property = computed(() => mockProperties.find((item) => item.id === route.params.id) ?? mockProperties[0]);
</script>

<template>
  <section class="page">
    <PageHeader :title="property.title" :description="property.address">
      <template #actions>
        <StatusBadge :label="property.status" variant="success" />
      </template>
    </PageHeader>

    <div class="detail-grid">
      <img class="detail-grid__image" :src="property.imageUrl" :alt="property.title" />
      <section class="panel">
        <div class="panel__body detail-grid__summary">
          <span>Giá bán</span>
          <strong class="numeric">{{ formatCurrency(property.price) }}</strong>
          <dl>
            <div>
              <dt>Diện tích</dt>
              <dd>{{ property.acreage }} m²</dd>
            </div>
            <div>
              <dt>Phòng ngủ</dt>
              <dd>{{ property.bedrooms }}</dd>
            </div>
            <div>
              <dt>Nguồn</dt>
              <dd class="mono">{{ property.source }}</dd>
            </div>
            <div>
              <dt>AI score</dt>
              <dd>{{ property.aiScore }}%</dd>
            </div>
          </dl>
        </div>
      </section>
    </div>
  </section>
</template>

<style scoped>
.detail-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: minmax(0, 1fr) 360px;
}

.detail-grid__image {
  aspect-ratio: 16 / 10;
  border-radius: 12px;
  object-fit: cover;
  width: 100%;
}

.detail-grid__summary span {
  color: var(--color-text-secondary);
}

.detail-grid__summary strong {
  display: block;
  font-size: 28px;
  margin: 8px 0 24px;
}

.detail-grid__summary dl {
  display: grid;
  gap: 16px;
  margin: 0;
}

.detail-grid__summary dt {
  color: var(--color-text-muted);
}

.detail-grid__summary dd {
  font-weight: 700;
  margin: 4px 0 0;
}

@media (max-width: 900px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>
