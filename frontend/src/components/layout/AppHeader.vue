<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import { Bell, Search } from '@element-plus/icons-vue';

import { useAuthStore } from '@/stores/useAuthStore';
import { useToast } from '@/composables/useToast';
import NotificationFeed from '@/components/common/NotificationFeed.vue';
import { mockNotifications } from '@/utils/mockData';

const emit = defineEmits<{
  'open-command-palette': [];
}>();

const authStore = useAuthStore();
const route = useRoute();
const toast = useToast();

const routeTitle = computed(() => String(route.meta.title ?? 'Dashboard'));

// Breadcrumb generation from route
const breadcrumbs = computed(() => {
  const crumbs = [{ label: 'Admin', href: '/admin' }];

  if (route.path === '/admin/dashboard' || route.path === '/admin') {
    return crumbs;
  }

  if (routeTitle.value && routeTitle.value !== 'Admin') {
    crumbs.push({ label: routeTitle.value, href: route.path });
  }

  return crumbs;
});

const handleSearch = () => {
  emit('open-command-palette');
};
</script>

<template>
  <header class="app-header">
    <nav class="breadcrumb" aria-label="breadcrumb">
      <ol class="breadcrumb__list">
        <li v-for="(crumb, idx) in breadcrumbs" :key="crumb.href" class="breadcrumb__item">
          <RouterLink :to="crumb.href" class="breadcrumb__link">
            {{ crumb.label }}
          </RouterLink>
          <span v-if="idx < breadcrumbs.length - 1" class="breadcrumb__sep">/</span>
        </li>
      </ol>
    </nav>

    <div class="app-header__search">
      <el-icon><Search /></el-icon>
      <input
        aria-label="Tìm kiếm"
        placeholder="Tìm kiếm... ⌘K"
        readonly
        @click="handleSearch"
      />
    </div>

    <div class="app-header__actions">
      <el-popover placement="bottom-end" :width="360" trigger="click">
        <template #reference>
          <el-button :icon="Bell" circle text />
        </template>
        <NotificationFeed :items="mockNotifications" />
      </el-popover>
      <div class="app-header__user">
        <span>{{ authStore.user?.fullName ?? 'Guest' }}</span>
        <small>{{ authStore.user?.role ?? 'Viewer' }}</small>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  align-items: center;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  display: flex;
  gap: 24px;
  height: 56px;
  justify-content: space-between;
  padding: 0 32px;
  position: sticky;
  top: 0;
  z-index: var(--z-sticky);
}

.breadcrumb {
  flex: 0 0 auto;
  min-width: 0;
}

.breadcrumb__list {
  display: flex;
  gap: 4px;
  list-style: none;
  margin: 0;
  padding: 0;
  align-items: center;
}

.breadcrumb__item {
  display: flex;
  gap: 4px;
  align-items: center;
}

.breadcrumb__link {
  font-size: 13px;
  font-weight: 500;
  color: var(--color-text-secondary);
  transition: color 200ms var(--ease-standard);
}

.breadcrumb__link:hover {
  color: var(--color-text-primary);
}

.breadcrumb__link[aria-current="page"],
.breadcrumb__item:last-child .breadcrumb__link {
  color: var(--color-text-primary);
  font-weight: 600;
  pointer-events: none;
}

.breadcrumb__sep {
  color: var(--color-text-muted);
}

.app-header__search {
  align-items: center;
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  color: var(--color-text-muted);
  cursor: text;
  display: flex;
  gap: 8px;
  max-width: 280px;
  padding: 0 12px;
  width: 100%;
  transition: border-color 200ms var(--ease-standard);
}

.app-header__search:hover {
  border-color: var(--color-border-strong);
}

.app-header__search:focus-within {
  border-color: var(--ring-color);
  outline: 2px solid var(--ring-color);
  outline-offset: var(--ring-offset);
}

.app-header__search input {
  background: transparent;
  border: 0;
  color: var(--color-text-primary);
  height: 38px;
  width: 100%;
  font-size: 13px;
}

.app-header__search input::placeholder {
  color: var(--color-text-muted);
}

.app-header__search input:focus-visible {
  outline: 0;
}

.app-header__actions {
  align-items: center;
  display: flex;
  gap: 16px;
}

.app-header__user {
  display: flex;
  flex-direction: column;
  font-weight: 600;
  line-height: 1.3;
  font-size: 13px;
}

.app-header__user small {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 500;
}

@media (max-width: 1024px) {
  .app-header {
    padding: 0 24px;
  }

  .app-header__search {
    max-width: 240px;
  }
}

@media (max-width: 640px) {
  .app-header {
    padding: 0 16px;
    gap: 16px;
  }

  .breadcrumb__list {
    max-width: 100px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .app-header__search {
    display: none;
  }

  .app-header__user small {
    display: none;
  }
}
</style>
