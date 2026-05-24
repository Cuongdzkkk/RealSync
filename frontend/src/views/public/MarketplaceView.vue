<script setup lang="ts">
import PropertyCard from '@/components/property/PropertyCard.vue';
import { mockProperties } from '@/utils/mockData';
</script>

<template>
  <main class="marketplace">
    <section class="marketplace__hero">
      <div>
        <span>RealSync Marketplace</span>
        <h1>Bất động sản đã được chuẩn hóa dữ liệu</h1>
        <p>Khám phá các listing nổi bật với điểm AI, nguồn dữ liệu rõ ràng và thông tin được đội vận hành kiểm chứng.</p>
        <div class="marketplace__actions">
          <a href="#properties">Xem sản phẩm</a>
          <RouterLink to="/admin/dashboard">Vào dashboard</RouterLink>
        </div>
      </div>
    </section>

    <section id="properties" class="marketplace__section">
      <div class="marketplace__section-header">
        <h2>Listing nổi bật</h2>
        <p>Dữ liệu mẫu cho showroom public của RealSync.</p>
      </div>
      <div class="marketplace__grid">
        <PropertyCard v-for="property in mockProperties" :key="property.id" :property="property" />
      </div>
    </section>
  </main>
</template>

<style scoped>
.marketplace__hero {
  align-items: center;
  background:
    linear-gradient(rgba(250, 250, 248, 0.82), rgba(250, 250, 248, 0.96)),
    url('https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?auto=format&fit=crop&w=1800&q=80') center / cover;
  display: flex;
  min-height: calc(100vh - 72px);
  padding: 64px 32px;
}

.marketplace__hero div {
  max-width: 760px;
}

.marketplace__hero span {
  color: var(--color-gold-dark);
  font-weight: 800;
}

.marketplace__hero h1 {
  color: var(--color-primary);
  font-family: var(--font-display);
  font-size: clamp(48px, 6vw, 72px);
  font-weight: 400;
  letter-spacing: -0.02em;
  line-height: 1.05;
  margin: 12px 0 20px;
  max-width: 760px;
}

.marketplace__hero p {
  color: var(--color-text-secondary);
  font-size: 18px;
  line-height: 1.7;
  max-width: 640px;
}

.marketplace__actions {
  display: flex;
  gap: 12px;
  margin-top: 28px;
}

.marketplace__actions a {
  background: var(--color-primary);
  border-radius: 20px;
  color: white;
  font-size: 15px;
  font-weight: 700;
  padding: 14px 28px;
  transition: transform var(--duration-base) var(--ease-spring), box-shadow var(--duration-base) var(--ease-standard);
}

.marketplace__actions a:hover {
  box-shadow: 0 8px 24px rgba(13, 27, 42, 0.2);
  transform: scale(1.02);
}

.marketplace__actions a + a {
  background: var(--color-gold);
  color: var(--color-primary);
}

.marketplace__section {
  margin: 0 auto;
  max-width: 1280px;
  padding: 80px 24px;
}

.marketplace__section-header {
  margin-bottom: 24px;
}

.marketplace__section-header h2 {
  font-family: var(--font-display);
  font-size: 36px;
  font-weight: 600;
  letter-spacing: -0.01em;
  line-height: 1.15;
  margin: 0 0 8px;
}

.marketplace__section-header p {
  color: var(--color-text-secondary);
  margin: 0;
}

.marketplace__grid {
  display: grid;
  gap: 24px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

@media (max-width: 900px) {
  .marketplace__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .marketplace__hero {
    min-height: auto;
    padding: 48px 20px;
  }

  .marketplace__hero h1 {
    font-size: clamp(36px, 10vw, 72px);
  }

  .marketplace__actions,
  .marketplace__grid {
    grid-template-columns: 1fr;
  }

  .marketplace__actions {
    flex-direction: column;
  }
}
</style>
