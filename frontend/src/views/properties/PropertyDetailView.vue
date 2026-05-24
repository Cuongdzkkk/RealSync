<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

import PageHeader from '@/components/common/PageHeader.vue';
import StatusBadge from '@/components/common/StatusBadge.vue';
import { mockProperties } from '@/utils/mockData';
import { formatCurrency, formatDate } from '@/utils/format';

const route = useRoute();
const property = computed(() => mockProperties.find(p => p.id === route.params.id));

function statusLabel(s: string) {
  const map: Record<string, string> = { published: 'Đang bán', verified: 'Đã xác thực', pending: 'Chờ duyệt', draft: 'Nháp' };
  return map[s] || s;
}

function statusVariant(s: string) {
  const map: Record<string, 'success' | 'info' | 'warning' | 'muted'> = { published: 'success', verified: 'info', pending: 'warning', draft: 'muted' };
  return map[s] || 'muted';
}
</script>

<template>
  <section class="page">
    <template v-if="property">
      <PageHeader :title="property.title">
        <template #back>
          <router-link to="/admin/properties" class="back-link">
            <span class="back-arrow">←</span>
            <span>Quay lại</span>
          </router-link>
        </template>
      </PageHeader>

      <div class="detail-grid">
        <div class="detail-col">
          <div class="panel detail-hero">
            <img :src="property.imageUrl" :alt="property.title" class="detail-hero__img" />
            <div class="detail-hero__overlay">
              <span class="detail-hero__price">{{ formatCurrency(property.price) }}</span>
              <StatusBadge :label="statusLabel(property.status)" :variant="statusVariant(property.status)" />
            </div>
          </div>

          <div class="panel detail-body">
            <dl class="detail-info">
              <div class="detail-info__row">
                <dt>Giá</dt>
                <dd>{{ formatCurrency(property.price) }}</dd>
              </div>
              <div class="detail-info__row">
                <dt>Diện tích</dt>
                <dd>{{ property.acreage }} m²</dd>
              </div>
              <div class="detail-info__row">
                <dt>Khu vực</dt>
                <dd>{{ property.area }}</dd>
              </div>
              <div class="detail-info__row">
                <dt>Địa chỉ</dt>
                <dd>{{ property.address }}</dd>
              </div>
              <div class="detail-info__row">
                <dt>Phòng ngủ</dt>
                <dd>{{ property.bedrooms }}</dd>
              </div>
              <div class="detail-info__row">
                <dt>Ngày đăng</dt>
                <dd>{{ formatDate(property.createdAt) }}</dd>
              </div>
              <div class="detail-info__row">
                <dt>Nguồn</dt>
                <dd>{{ property.source }}</dd>
              </div>
              <div class="detail-info__row">
                <dt>AI Score</dt>
                <dd class="numeric">{{ property.aiScore }}%</dd>
              </div>
            </dl>
          </div>
        </div>

        <div class="detail-col detail-col--side">
          <div class="panel detail-actions">
            <button class="detail-btn detail-btn--primary">Chỉnh sửa</button>
            <button class="detail-btn">Ẩn tin</button>
          </div>
        </div>
      </div>
    </template>

    <div v-else class="detail-empty">
      <p>Bất động sản không tồn tại.</p>
      <router-link to="/admin/properties" class="back-link">← Quay lại danh sách</router-link>
    </div>
  </section>
</template>

<style scoped>
.detail-grid {
  display: grid;
  grid-template-columns: 1fr 260px;
  gap: 24px;
  align-items: start;
}

.detail-hero {
  position: relative;
  overflow: hidden;
  margin-bottom: 24px;
  padding: 0;
}

.detail-hero__img {
  width: 100%;
  height: 300px;
  object-fit: cover;
  display: block;
}

.detail-hero__overlay {
  position: absolute;
  bottom: 12px;
  left: 12px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.detail-hero__price {
  font-size: 20px;
  font-weight: 700;
  color: #FFFFFF;
  background: rgba(0, 0, 0, 0.65);
  padding: 5px 12px;
  border-radius: 6px;
}

.detail-body {
  padding: 24px;
}

.detail-info {
  margin: 0;
}

.detail-info__row {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid var(--color-divider);
  font-size: 13px;
  gap: 16px;
}

.detail-info__row dt {
  color: var(--color-text-muted);
  font-weight: 400;
  white-space: nowrap;
}

.detail-info__row dd {
  margin: 0;
  font-weight: 500;
  color: var(--color-text-primary);
  text-align: right;
}

.detail-info__desc {
  max-width: 320px;
  text-align: left !important;
  line-height: 1.6;
}

.detail-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--color-divider);
}

.detail-tag {
  background: var(--color-tag-bg);
  color: var(--color-tag-text);
  font-size: 11px;
  padding: 4px 10px;
  border-radius: 4px;
  font-weight: 500;
}

.detail-actions {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-btn {
  width: 100%;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-primary);
  cursor: pointer;
  text-align: center;
  transition: background var(--duration-fast) var(--ease-standard);
}

.detail-btn:hover {
  background: var(--color-surface-hover);
}

.detail-btn--primary {
  background: #0D0D0D;
  color: #FFFFFF;
  border-color: #0D0D0D;
}

.detail-btn--primary:hover {
  background: #333333;
}

.detail-empty {
  text-align: center;
  padding: 80px 24px;
  color: var(--color-text-secondary);
}

.back-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--color-text-secondary);
  text-decoration: none;
}

.back-link:hover {
  color: var(--color-text-primary);
}
</style>
