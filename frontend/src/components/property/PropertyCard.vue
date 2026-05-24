<script setup lang="ts">
import type { Property } from '@/types/property';
import { formatCurrency } from '@/utils/format';

defineProps<{
  property: Property;
}>();
</script>

<template>
  <article class="property-card">
    <div class="property-card__media">
      <img :src="property.imageUrl" :alt="property.title" />
      <span class="property-card__badge">{{ property.status }}</span>
    </div>
    <div class="property-card__body">
      <strong class="numeric">{{ formatCurrency(property.price) }}</strong>
      <h3>{{ property.title }}</h3>
      <p>{{ property.address }}</p>
      <div class="property-card__specs">
        <span>{{ property.acreage }} m²</span>
        <span>{{ property.bedrooms }} PN</span>
      </div>
    </div>
  </article>
</template>

<style scoped>
.property-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  cursor: pointer;
  overflow: hidden;
  transition: border-color var(--duration-base) var(--ease-standard),
              box-shadow var(--duration-base) var(--ease-standard);
}

.property-card:hover {
  border-color: var(--color-border-strong);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
}

.property-card__media {
  aspect-ratio: 4 / 3;
  position: relative;
}

.property-card__media img {
  height: 100%;
  object-fit: cover;
  width: 100%;
}

.property-card__badge {
  position: absolute;
  bottom: 8px;
  left: 8px;
  background: rgba(13, 13, 13, 0.75);
  color: #FFFFFF;
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  padding: 2px 6px;
  border-radius: 4px;
  line-height: 1.2;
}

.property-card__body {
  padding: 14px 16px;
}

.property-card__body strong {
  color: var(--color-text-primary);
  display: block;
  font-size: 16px;
  font-weight: 700;
  letter-spacing: -0.01em;
  line-height: 1.2;
  margin-bottom: 6px;
}

.property-card__body h3 {
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 4px 0;
}

.property-card__body p {
  font-size: 12px;
  font-weight: 400;
  color: var(--color-text-secondary);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.property-card__specs {
  display: flex;
  gap: 10px;
  margin-top: 10px;
  font-size: 12px;
  font-weight: 500;
  color: var(--color-text-secondary);
}
</style>
