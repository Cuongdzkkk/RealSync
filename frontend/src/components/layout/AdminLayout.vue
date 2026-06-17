<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useAppStore } from '@/stores/useAppStore';
import AppSidebar from './AppSidebar.vue';
import AppTopbar from './AppTopbar.vue';
import ActivityTimelinePanel from './ActivityTimelinePanel.vue';
import FloatingAiAssistant from './FloatingAiAssistant.vue';
import CommandPalette from '@/components/common/CommandPalette.vue';
import ToastContainer from '@/components/common/ToastContainer.vue';

const appStore = useAppStore();
const route = useRoute();
const commandPaletteOpen = ref(false);
const timelineOpen = ref(true);

watch(
  () => route.fullPath,
  () => {
    timelineOpen.value = route.meta.defaultTimelineOpen ?? true;
  },
  { immediate: true }
);

function openCommandPalette() {
  commandPaletteOpen.value = true;
}

function closeCommandPalette() {
  commandPaletteOpen.value = false;
}

function toggleTimeline() {
  timelineOpen.value = !timelineOpen.value;
}

function handleGlobalKeydown(e: KeyboardEvent) {
  if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
    e.preventDefault();
    commandPaletteOpen.value = !commandPaletteOpen.value;
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleGlobalKeydown);
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleGlobalKeydown);
});
</script>

<template>
  <div class="admin-shell" :class="appStore.theme">
    <!-- App Sidebar (Collapsible Mini Sidebar) -->
    <AppSidebar />

    <div class="admin-main">
      <!-- Sticky Topbar -->
      <AppTopbar 
        @open-command-palette="openCommandPalette" 
        @toggle-timeline="toggleTimeline"
      />

      <!-- Asymmetric Layout Content Area -->
      <div class="admin-content" :class="{ 'timeline-hidden': !timelineOpen }">
        <!-- Center/Main Dashboard View -->
        <main class="main-viewport">
          <RouterView />
        </main>

        <!-- Right Side Panel: Activity Timeline (Collapsible) -->
        <aside v-if="timelineOpen" class="timeline-viewport glass-card">
          <button class="timeline-close-btn" @click="toggleTimeline" title="Ẩn hoạt động">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
          <ActivityTimelinePanel />
        </aside>
      </div>

      <!-- Quick toggle button when Timeline is closed -->
      <button 
        v-if="!timelineOpen" 
        class="timeline-toggle-btn glass-card glow-yellow"
        @click="toggleTimeline"
        title="Hiện hoạt động hệ thống"
      >
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="12 8 8 12 12 16" />
          <line x1="16" y1="12" x2="8" y2="12" />
        </svg>
      </button>
    </div>

    <!-- Floating AI Assistant Panel -->
    <FloatingAiAssistant />

    <!-- Command Palette (Linear/Notion search style) -->
    <CommandPalette
      v-if="commandPaletteOpen"
      @close="closeCommandPalette"
      @select="closeCommandPalette"
    />
    <ToastContainer />
  </div>
</template>

<style scoped>
.admin-shell {
  display: flex;
  min-height: 100dvh;
  background: var(--color-canvas);
  background-image: var(--color-mesh-bg);
  background-attachment: fixed;
  transition: background var(--duration-base) var(--ease-standard);
  position: relative;
}

.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  position: relative;
}

.admin-content {
  display: grid;
  grid-template-columns: 1fr 320px;
  flex: 1;
  min-height: calc(100dvh - 60px);
  position: relative;
  transition: all var(--duration-base) var(--ease-standard);
}

.admin-content.timeline-hidden {
  grid-template-columns: 1fr;
}

.main-viewport {
  padding: 24px;
  overflow-y: auto;
  min-width: 0;
}

.timeline-viewport {
  border-left: 1px solid var(--color-border);
  border-right: none;
  border-top: none;
  border-bottom: none;
  border-radius: 0;
  background: var(--color-sidebar-bg);
  padding: 24px 20px;
  height: calc(100dvh - 60px);
  position: sticky;
  top: 60px;
  overflow-y: auto;
  box-shadow: none;
  transition: all var(--duration-base) var(--ease-standard);
}

.timeline-close-btn {
  position: absolute;
  top: 20px;
  right: 20px;
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
}

.timeline-close-btn:hover {
  background: var(--color-surface-hover);
  color: var(--color-text-primary);
}

.timeline-toggle-btn {
  position: fixed;
  top: 76px;
  right: 20px;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: var(--elevation-surface);
  transition: all var(--duration-fast);
}

.timeline-toggle-btn:hover {
  background: var(--color-yellow);
  color: var(--color-yellow-text);
  transform: translateX(-2px);
}

@media (max-width: 1024px) {
  .admin-content {
    grid-template-columns: 1fr;
  }
  .timeline-viewport {
    display: none;
  }
  .timeline-toggle-btn {
    display: none;
  }
}
</style>
