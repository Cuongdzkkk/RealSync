<script setup lang="ts">
import { ref, computed } from 'vue';
import { Collection, Cpu, DataAnalysis, Document, House, Link, OfficeBuilding, Setting, TrendCharts, User, CaretRight } from '@element-plus/icons-vue';

const isCollapsed = ref(false);

const navGroups = [
  {
    label: 'TỔNG QUAN',
    items: [
      { label: 'Dashboard', to: '/admin/dashboard', icon: DataAnalysis },
    ]
  },
  {
    label: 'DỮ LIỆU',
    items: [
      { label: 'Crawler', to: '/admin/crawlers', icon: Link },
      { label: 'Sản phẩm', to: '/admin/properties', icon: House },
      { label: 'Dự án', to: '/admin/projects', icon: OfficeBuilding },
    ]
  },
  {
    label: 'KHÁCH HÀNG',
    items: [
      { label: 'Leads', to: '/admin/leads', icon: User },
      { label: 'AI Classify', to: '/admin/ai-classification', icon: Cpu, ai: true },
    ]
  },
  {
    label: 'NỘI DUNG',
    items: [
      { label: 'Content AI', to: '/admin/content-ai', icon: Document, ai: true },
    ]
  },
  {
    label: 'HỆ THỐNG',
    items: [
      { label: 'Insights', to: '/admin/insights', icon: TrendCharts },
      { label: 'Users & Roles', to: '/admin/users', icon: Collection },
      { label: 'Cài đặt', to: '/admin/settings', icon: Setting },
    ]
  }
];

const toggleCollapse = () => {
  isCollapsed.value = !isCollapsed.value;
};

const sidebarWidth = computed(() => (isCollapsed.value ? '64px' : '240px'));
</script>

<template>
  <aside class="sidebar" :class="{ 'sidebar--collapsed': isCollapsed }">
    <!-- Logo zone -->
    <div class="sidebar__header">
      <RouterLink class="sidebar__brand" to="/admin/dashboard">
        <span class="sidebar__mark">RS</span>
        <span v-show="!isCollapsed" class="sidebar__name">RealSync</span>
      </RouterLink>
    </div>

    <!-- Navigation -->
    <nav class="sidebar__nav">
      <section v-for="group in navGroups" :key="group.label" class="sidebar__group">
        <p v-show="!isCollapsed" class="sidebar__label">{{ group.label }}</p>
        <RouterLink
          v-for="item in group.items"
          :key="item.label"
          class="sidebar__item"
          :class="{ 'sidebar__item--ai': item.ai }"
          :to="item.to"
          :title="isCollapsed ? item.label : undefined"
        >
          <component :is="item.icon" class="sidebar__icon" />
          <span v-show="!isCollapsed">{{ item.label }}</span>
        </RouterLink>
      </section>
    </nav>

    <!-- Footer: collapse toggle -->
    <div class="sidebar__footer">
      <button
        class="sidebar__collapse-btn"
        :title="isCollapsed ? 'Expand' : 'Collapse'"
        :aria-label="isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
        @click="toggleCollapse"
      >
        <CaretRight class="sidebar__collapse-icon" />
      </button>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  background: var(--color-primary);
  color: white;
  flex: 0 0 240px;
  min-height: 100dvh;
  z-index: var(--z-sidebar);
  display: flex;
  flex-direction: column;
  transition: flex-basis var(--duration-slow) var(--ease-standard);
  overflow: hidden;
}

.sidebar--collapsed {
  flex-basis: 64px;
}

/* Header */
.sidebar__header {
  flex: 0 0 56px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  align-items: center;
  padding: 0 12px;
}

.sidebar__brand {
  align-items: center;
  display: flex;
  gap: 12px;
  width: 100%;
  text-decoration: none;
  border-radius: 6px;
  padding: 8px;
  transition: background 200ms var(--ease-standard);
}

.sidebar__brand:hover {
  background: rgba(255, 255, 255, 0.1);
}

.sidebar__mark {
  align-items: center;
  background: var(--color-gold);
  border-radius: 6px;
  color: var(--color-primary);
  display: inline-flex;
  font-weight: 800;
  height: 32px;
  justify-content: center;
  width: 32px;
  flex-shrink: 0;
}

.sidebar__name {
  font-size: 15px;
  font-weight: 700;
  white-space: nowrap;
}

/* Navigation */
.sidebar__nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px 8px;
  overflow-y: auto;
}

.sidebar__group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.sidebar__label {
  color: rgba(255, 255, 255, 0.35);
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.04em;
  margin: 0;
  padding: 8px 12px 4px;
  text-transform: uppercase;
  white-space: nowrap;
}

.sidebar__item {
  align-items: center;
  border-left: 2px solid transparent;
  border-radius: 6px;
  color: rgba(255, 255, 255, 0.65);
  cursor: pointer;
  display: flex;
  font-size: 14px;
  font-weight: 500;
  gap: 12px;
  margin: 0 4px;
  padding: 10px 12px;
  text-decoration: none;
  transition: all 200ms var(--ease-standard);
  position: relative;
}

.sidebar__item:hover {
  background: rgba(255, 255, 255, 0.08);
  color: rgba(255, 255, 255, 0.9);
}

.sidebar__item.router-link-active {
  background: rgba(255, 255, 255, 0.1);
  border-left-color: var(--color-gold);
  color: white;
  font-weight: 600;
}

.sidebar__item--ai .sidebar__icon {
  color: var(--color-ai);
}

.sidebar__icon {
  height: 18px;
  width: 18px;
  flex-shrink: 0;
}

/* Footer */
.sidebar__footer {
  flex: 0 0 auto;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  padding: 12px 8px;
  display: flex;
  justify-content: center;
}

.sidebar__collapse-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 0;
  border-radius: 6px;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 36px;
  width: 36px;
  transition: background 200ms var(--ease-standard);
}

.sidebar__collapse-btn:hover {
  background: rgba(255, 255, 255, 0.15);
}

.sidebar__collapse-icon {
  height: 18px;
  width: 18px;
  transition: transform var(--duration-slow) var(--ease-standard);
}

.sidebar--collapsed .sidebar__collapse-icon {
  transform: rotate(180deg);
}

/* Responsive */
@media (max-width: 1024px) {
  .sidebar {
    flex-basis: 64px;
  }

  .sidebar__name,
  .sidebar__label,
  .sidebar__item span {
    display: none;
  }

  .sidebar__item {
    justify-content: center;
    margin: 0;
    padding: 10px;
  }

  .sidebar__collapse-btn {
    display: none;
  }
}

@media (max-width: 640px) {
  .sidebar {
    position: fixed;
    left: 0;
    top: 56px;
    height: calc(100dvh - 56px);
    flex-basis: 240px;
    z-index: var(--z-overlay);
    transform: translateX(-100%);
    transition: transform var(--duration-slow) var(--ease-standard);
    box-shadow: var(--elevation-floating);
  }

  .sidebar--open {
    transform: translateX(0);
  }
}

/* Scrollbar styling */
.sidebar__nav::-webkit-scrollbar {
  width: 6px;
}

.sidebar__nav::-webkit-scrollbar-track {
  background: transparent;
}

.sidebar__nav::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 3px;
}

.sidebar__nav::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.3);
}
</style>
