<script setup lang="ts">
import AppFooter from './AppFooter.vue';
</script>

<template>
  <div class="public-layout">
    <header class="public-nav">
      <RouterLink class="public-nav__brand" to="/">RealSync</RouterLink>
      <nav class="public-nav__links">
        <a href="#properties">Bất động sản</a>
        <a href="#areas">Khu vực</a>
        <RouterLink to="/login">Đăng nhập</RouterLink>
      </nav>
    </header>
    <RouterView />
    <AppFooter />
  </div>
</template>

<style scoped>
.public-layout {
  background: var(--color-canvas);
  min-height: 100dvh;
}

.public-nav {
  align-items: center;
  backdrop-filter: blur(12px);
  background: rgba(250, 250, 248, 0.92);
  border-bottom: 1px solid transparent;
  display: flex;
  height: 72px;
  justify-content: space-between;
  padding: 0 max(24px, calc((100vw - 1280px) / 2));
  position: sticky;
  top: 0;
  transition: border-color var(--duration-base) var(--ease-standard), box-shadow var(--duration-base) var(--ease-standard);
  z-index: var(--z-sticky);
}

.public-nav__brand {
  color: var(--color-primary);
  font-size: 20px;
  font-weight: 800;
}

.public-nav__links {
  align-items: center;
  display: flex;
  gap: 24px;
  font-weight: 700;
}

.public-nav__links a {
  border-radius: 6px;
  color: var(--color-text-secondary);
  font-size: 14px;
  padding: 6px 12px;
  transition: color var(--duration-fast) var(--ease-standard);
}

.public-nav__links a:hover {
  color: var(--color-primary);
}

@media (max-width: 640px) {
  .public-nav {
    padding: 0 16px;
  }

  .public-nav__links {
    gap: 12px;
  }
}
</style>
