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
      <span>{{ property.status }}</span>
    </div>
    <div class="property-card__body">
      <strong class="numeric">{{ formatCurrency(property.price) }}</strong>
      <h3>{{ property.title }}</h3>
      <p>{{ property.address }}</p>
      <dl>
        <div>
          <dt>Diện tích</dt>
          <dd>{{ property.acreage }} m²</dd>
        </div>
        <div>
          <dt>Phòng ngủ</dt>
          <dd>{{ property.bedrooms }}</dd>
        </div>
      </dl>
    </div>
  </article>
</template>

<style scoped>
.property-card {
  background: var(--color-surface);
  border-radius: 12px;
  box-shadow: var(--elevation-raised);
  cursor: pointer;
  overflow: hidden;
  transition: transform var(--duration-base) var(--ease-spring), box-shadow var(--duration-base) var(--ease-standard);
}

.property-card:hover {
  box-shadow: var(--elevation-floating);
  transform: translateY(-3px);
}

.property-card__media {
  aspect-ratio: 4 / 3;
  position: relative;
}

.property-card__media img {
  height: 100%;
  object-fit: cover;
  transition: transform var(--duration-slow) var(--ease-standard);
  width: 100%;
}

.property-card:hover .property-card__media img {
  transform: scale(1.03);
}

.property-card__media span {
  background: var(--color-primary);
  border-radius: 6px;
  color: white;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  left: 12px;
  padding: 4px 10px;
  position: absolute;
  text-transform: uppercase;
  top: 12px;
}

.property-card__body {
  padding: 16px;
}

.property-card__body strong {
  color: var(--color-primary);
  display: block;
  font-size: 22px;
  letter-spacing: -0.01em;
  line-height: 1;
  margin-bottom: 10px;
}

.property-card__body h3 {
  font-size: 16px;
  margin: 0 0 8px;
}

.property-card__body p {
  color: var(--color-text-secondary);
  margin: 0;
}

.property-card__body dl {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, 1fr);
  margin: 16px 0 0;
}

.property-card__body dt {
  color: var(--color-text-muted);
  font-size: 12px;
}

.property-card__body dd {
  font-weight: 700;
  margin: 4px 0 0;
}
</style>
