<script setup lang="ts">
import type { Property } from '@/types/property';
import { formatCurrency } from '@/utils/format';

defineProps<{
  property: Property;
}>();

const emit = defineEmits<{
  (e: 'click', id: string): void;
  (e: 'edit', property: Property): void;
  (e: 'delete', id: string): void;
}>();
</script>

<template>
  <article class="property-card glass-card glass-card--hoverable" @click="emit('click', property.id)">
    <!-- Media Section -->
    <div class="property-card__media">
      <img :src="property.imageUrl" :alt="property.title" loading="lazy" />
      
      <!-- Source Badge -->
      <span class="source-tag" :class="property.source.toLowerCase().replace('.', '-')">
        {{ property.source }}
      </span>

      <!-- AI score badge -->
      <div class="ai-match-badge glow-ai">
        <span class="sparkle">✨</span>
        <span class="score numeric">{{ property.aiScore }}%</span>
      </div>

      <!-- Status badge overlay -->
      <span class="status-tag" :class="property.status">
        {{ property.status === 'draft' ? 'Nháp' : property.status === 'verified' ? 'Đã duyệt' : property.status === 'published' ? 'Đã đăng' : 'Hết hạn' }}
      </span>
    </div>

    <!-- Body Section -->
    <div class="property-card__body">
      <div class="price-row">
        <strong class="price-val numeric">{{ formatCurrency(property.price) }}</strong>
        <span class="price-unit">/ total</span>
      </div>
      
      <h3 class="property-title" :title="property.title">{{ property.title }}</h3>
      <p class="property-address">
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" /><circle cx="12" cy="10" r="3" />
        </svg>
        <span>{{ property.address }}</span>
      </p>

      <!-- Specs row -->
      <div class="property-specs">
        <div class="spec-item">
          <!-- Area Icon -->
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="18" height="18" rx="2" /><line x1="9" y1="3" x2="9" y2="21" />
          </svg>
          <span class="numeric">{{ property.acreage }} m²</span>
        </div>
        <div class="spec-item">
          <!-- Bed Icon -->
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M2 4v16M2 8h20M2 17h20M22 4v16M2 12h20" />
          </svg>
          <span class="numeric">{{ property.bedrooms }} PN</span>
        </div>
      </div>
    </div>

    <!-- Interactive hover actions overlay -->
    <div class="card-action-overlay" @click.stop>
      <button class="action-btn action-btn--primary" @click="emit('click', property.id)">Xem Chi Tiết</button>
      <div class="action-subgroup">
        <button class="action-btn icon-only" @click="emit('edit', property)" title="Sửa">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4z" />
          </svg>
        </button>
        <button class="action-btn icon-only danger" @click="emit('delete', property.id)" title="Xóa">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
          </svg>
        </button>
      </div>
    </div>
  </article>
</template>

<style scoped>
.property-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
  position: relative;
  cursor: pointer;
}

.property-card__media {
  aspect-ratio: 16 / 10;
  position: relative;
  overflow: hidden;
  background-color: var(--color-divider);
}

.property-card__media img {
  height: 100%;
  object-fit: cover;
  width: 100%;
  transition: transform var(--duration-slow) var(--ease-standard);
}

.property-card:hover .property-card__media img {
  transform: scale(1.05);
}

/* Source tag */
.source-tag {
  position: absolute;
  top: 10px;
  left: 10px;
  background: rgba(15, 23, 42, 0.75);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: #ffffff;
  font-size: 9px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  z-index: 2;
}

.source-tag.batdongsan-com-vn { background-color: rgba(14, 165, 233, 0.8); }
.source-tag.chotot-com { background-color: rgba(245, 158, 11, 0.8); }
.source-tag.internal { background-color: rgba(16, 185, 129, 0.8); }

/* AI Match tag */
.ai-match-badge {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  border: 1px solid var(--color-ai-border);
  color: var(--color-ai);
  font-size: 10px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 4px;
  z-index: 2;
}

/* Status tag overlay */
.status-tag {
  position: absolute;
  bottom: 10px;
  left: 10px;
  font-size: 9px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #ffffff;
  z-index: 2;
}

.status-tag.draft { background-color: rgba(148, 163, 184, 0.8); }
.status-tag.verified { background-color: rgba(59, 130, 246, 0.8); }
.status-tag.published { background-color: rgba(16, 185, 129, 0.8); }
.status-tag.expired { background-color: rgba(239, 68, 68, 0.8); }

/* Card body */
.property-card__body {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.price-row {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.price-val {
  color: var(--color-text-primary);
  font-size: 17px;
  font-weight: 700;
  line-height: 1;
}

.price-unit {
  font-size: 10px;
  color: var(--color-text-muted);
}

.property-title {
  font-size: 13.5px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  height: 38px;
}

.property-address {
  font-size: 11.5px;
  color: var(--color-text-secondary);
  margin: 0;
  display: flex;
  align-items: center;
  gap: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.property-address svg {
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.property-specs {
  display: flex;
  gap: 16px;
  margin-top: auto;
  padding-top: 8px;
  border-top: 1px solid var(--color-divider);
}

.spec-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11.5px;
  color: var(--color-text-secondary);
  font-weight: 600;
}

.spec-item svg {
  color: var(--color-text-muted);
}

/* Actions Overlay on Hover */
.card-action-overlay {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(15, 23, 42, 0.85);
  backdrop-filter: blur(4px);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  opacity: 0;
  transition: opacity var(--duration-fast);
  border-radius: inherit;
  z-index: 5;
  padding: 20px;
}

.property-card:hover .card-action-overlay {
  opacity: 1;
}

.action-btn {
  font-size: 12px;
  font-weight: 600;
  height: 36px;
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.15);
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 16px;
  transition: all var(--duration-fast);
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.25);
  border-color: rgba(255, 255, 255, 0.3);
}

.action-btn--primary {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  border: none;
  box-shadow: var(--color-yellow-glow);
  width: 120px;
}
.action-btn--primary:hover {
  background: var(--color-yellow-hover);
}

.action-subgroup {
  display: flex;
  gap: 8px;
}

.action-btn.icon-only {
  width: 36px;
  padding: 0;
}

.action-btn.icon-only.danger:hover {
  background-color: var(--color-danger);
  border-color: var(--color-danger);
}
</style>
