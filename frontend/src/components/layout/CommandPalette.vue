<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from '@/composables/useToast';

const emit = defineEmits<{
  close: [];
}>();

const router = useRouter();
const toast = useToast();

const searchQuery = ref('');
const selectedIndex = ref(0);

const commands = [
  {
    id: 'dashboard',
    label: 'Tổng quan',
    category: 'Navigation',
    shortcut: '⌘D',
    action: () => router.push('/admin/dashboard'),
  },
  {
    id: 'new-lead',
    label: 'Tạo Lead mới',
    category: 'Actions',
    shortcut: '⌘N',
    action: () => toast.info('Create Lead', 'Feature coming soon'),
  },
  {
    id: 'crawler',
    label: 'Chạy Crawler',
    category: 'Actions',
    shortcut: '⌘R',
    action: () => router.push('/admin/crawlers'),
  },
  {
    id: 'ai-classify',
    label: 'AI Classify Leads',
    category: 'AI',
    shortcut: '⌘A',
    action: () => router.push('/admin/ai-classification'),
  },
  {
    id: 'content-ai',
    label: 'Content AI',
    category: 'AI',
    shortcut: '⌘C',
    action: () => router.push('/admin/content-ai'),
  },
  {
    id: 'leads',
    label: 'Danh sách Leads',
    category: 'Navigation',
    shortcut: '⌘L',
    action: () => router.push('/admin/leads'),
  },
  {
    id: 'properties',
    label: 'Danh sách Sản phẩm',
    category: 'Navigation',
    shortcut: '⌘P',
    action: () => router.push('/admin/properties'),
  },
  {
    id: 'settings',
    label: 'Cài đặt',
    category: 'System',
    shortcut: '⌘,',
    action: () => router.push('/admin/settings'),
  },
];

// Fuzzy search
const fuzzyMatch = (query: string, text: string): boolean => {
  if (!query) return true;
  const lowerQuery = query.toLowerCase();
  const lowerText = text.toLowerCase();
  let queryIdx = 0;
  for (let i = 0; i < lowerText.length && queryIdx < lowerQuery.length; i++) {
    if (lowerText[i] === lowerQuery[queryIdx]) {
      queryIdx++;
    }
  }
  return queryIdx === lowerQuery.length;
};

const filteredCommands = computed(() => {
  if (!searchQuery.value) {
    return commands;
  }
  return commands.filter((cmd) =>
    fuzzyMatch(searchQuery.value, cmd.label) || fuzzyMatch(searchQuery.value, cmd.category)
  );
});

const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') {
    emit('close');
  } else if (e.key === 'ArrowDown') {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value + 1) % Math.max(1, filteredCommands.value.length);
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    selectedIndex.value =
      (selectedIndex.value - 1 + Math.max(1, filteredCommands.value.length)) %
      Math.max(1, filteredCommands.value.length);
  } else if (e.key === 'Enter') {
    e.preventDefault();
    if (filteredCommands.value[selectedIndex.value]) {
      selectCommand(filteredCommands.value[selectedIndex.value]);
    }
  }
};

const selectCommand = (cmd: typeof commands[0]) => {
  cmd.action();
  emit('close');
};

onMounted(() => {
  document.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeyDown);
});

// Group commands by category
const groupedCommands = computed(() => {
  const grouped: Record<string, typeof commands> = {};
  for (const cmd of filteredCommands.value) {
    if (!grouped[cmd.category]) {
      grouped[cmd.category] = [];
    }
    grouped[cmd.category].push(cmd);
  }
  return grouped;
});
</script>

<template>
  <div class="palette-overlay" @click.self="emit('close')">
    <div class="palette-modal">
      <!-- Search input -->
      <div class="palette-input">
        <input
          v-model="searchQuery"
          autofocus
          type="text"
          placeholder="Tìm kiếm lệnh..."
          @keydown="handleKeyDown"
        />
      </div>

      <!-- Results -->
      <div class="palette-results">
        <template v-if="filteredCommands.length > 0">
          <div
            v-for="(cmds, category) in groupedCommands"
            :key="category"
            class="palette-group"
          >
            <div class="palette-group__header">{{ category }}</div>
            <button
              v-for="(cmd, idx) in cmds"
              :key="cmd.id"
              class="palette-command"
              :class="{ 'palette-command--selected': selectedIndex === commands.indexOf(cmd) }"
              @click="selectCommand(cmd)"
              @mouseenter="selectedIndex = commands.indexOf(cmd)"
            >
              <span class="palette-command__label">{{ cmd.label }}</span>
              <span class="palette-command__shortcut">{{ cmd.shortcut }}</span>
            </button>
          </div>
        </template>
        <div v-else class="palette-empty">
          <p>Không tìm thấy lệnh</p>
        </div>
      </div>

      <!-- Help text -->
      <div class="palette-help">
        <kbd>↑↓</kbd> di chuyển · <kbd>Enter</kbd> chọn · <kbd>Esc</kbd> đóng
      </div>
    </div>
  </div>
</template>

<style scoped>
.palette-overlay {
  position: fixed;
  inset: 0;
  background: rgba(13, 27, 42, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding-top: 15%;
  z-index: var(--z-modal);
  animation: fadeIn 200ms var(--ease-standard);
}

.palette-modal {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  box-shadow: var(--elevation-floating);
  width: 100%;
  max-width: 560px;
  max-height: 70vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.palette-input {
  flex: 0 0 auto;
  padding: 16px 20px;
  border-bottom: 1px solid var(--color-border);
}

.palette-input input {
  width: 100%;
  background: transparent;
  border: 0;
  color: var(--color-text-primary);
  font-size: 16px;
  font-weight: 500;
  outline: 0;
}

.palette-input input::placeholder {
  color: var(--color-text-muted);
}

.palette-results {
  flex: 1;
  overflow-y: auto;
  padding: 8px 0;
}

.palette-group {
  display: flex;
  flex-direction: column;
}

.palette-group__header {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--color-text-muted);
  padding: 8px 16px 4px;
}

.palette-command {
  background: transparent;
  border: 0;
  color: var(--color-text-primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  font-size: 14px;
  transition: background-color 120ms var(--ease-standard);
}

.palette-command:hover,
.palette-command--selected {
  background-color: var(--color-canvas);
}

.palette-command__label {
  font-weight: 500;
}

.palette-command__shortcut {
  font-size: 11px;
  color: var(--color-text-muted);
  font-family: var(--font-mono);
}

.palette-empty {
  padding: 40px 20px;
  text-align: center;
  color: var(--color-text-muted);
}

.palette-help {
  flex: 0 0 auto;
  border-top: 1px solid var(--color-border);
  padding: 8px 16px;
  font-size: 12px;
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  gap: 8px;
}

.palette-help kbd {
  background: var(--color-canvas);
  border: 1px solid var(--color-border);
  border-radius: 4px;
  padding: 2px 6px;
  font-family: var(--font-mono);
  font-size: 11px;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@media (max-width: 640px) {
  .palette-overlay {
    padding-top: 0;
    align-items: flex-end;
  }

  .palette-modal {
    width: 100%;
    max-width: none;
    max-height: 80vh;
    border-radius: 12px 12px 0 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .palette-overlay,
  .palette-command {
    animation: none !important;
    transition: none !important;
  }
}
</style>
