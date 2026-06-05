<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import {
  Cpu, Document, Link, Plus, Setting, Switch, Top
} from '@element-plus/icons-vue';
import { useAppStore } from '@/stores/useAppStore';

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'select', command: Command): void;
}>();

const router = useRouter();
const appStore = useAppStore();

interface Command {
  id: string;
  label: string;
  icon: unknown;
  category: string;
  action: () => void;
  shortcut?: string;
}

const query = ref('');
const activeIndex = ref(0);
const inputRef = ref<HTMLInputElement | null>(null);

const commands: Command[] = [
  {
    id: 'new-lead',
    label: 'Tạo lead mới',
    icon: Plus,
    category: 'Hành động',
    action: () => router.push('/admin/leads'),
    shortcut: 'L'
  },
  {
    id: 'run-crawler',
    label: 'Chạy crawler',
    icon: Link,
    category: 'Hành động',
    action: () => router.push('/admin/crawlers')
  },
  {
    id: 'ai-classify',
    label: 'Phân loại AI khách hàng',
    icon: Cpu,
    category: 'Điều hướng',
    action: () => router.push('/admin/ai-classification')
  },
  {
    id: 'ai-content',
    label: 'AI Content',
    icon: Document,
    category: 'Điều hướng',
    action: () => router.push('/admin/content-ai')
  },
  {
    id: 'dashboard',
    label: 'Tổng quan',
    icon: Top,
    category: 'Điều hướng',
    action: () => router.push('/admin/dashboard'),
    shortcut: 'D'
  },
  {
    id: 'leads',
    label: 'Danh sách khách hàng',
    icon: Switch,
    category: 'Điều hướng',
    action: () => router.push('/admin/leads'),
    shortcut: 'L'
  },
  {
    id: 'settings',
    label: 'Cài đặt',
    icon: Setting,
    category: 'Hành động',
    action: () => router.push('/admin/settings')
  },
  {
    id: 'export',
    label: 'Xuất danh sách',
    icon: Document,
    category: 'Hành động',
    action: () => {
      emit('close');
    }
  }
];

const filtered = () => {
  if (!query.value) return commands;
  const q = removeVietnameseTones(query.value.toLowerCase());
  return commands.filter((cmd) => {
    return removeVietnameseTones(cmd.label.toLowerCase()).includes(q);
  });
};

function removeVietnameseTones(s: string): string {
  return s
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'D');
}

const visibleCommands = filtered();
const groupedCommands = () => {
  const groups: { category: string; items: Command[] }[] = [];
  for (const cmd of visibleCommands) {
    let group = groups.find((g) => g.category === cmd.category);
    if (!group) {
      group = { category: cmd.category, items: [] };
      groups.push(group);
    }
    group.items.push(cmd);
  }
  return groups;
};

function selectCommand(cmd: Command) {
  cmd.action();
  emit('select', cmd);
  emit('close');
}//hihaha


function onKeydown(e: KeyboardEvent) {
  const items = visibleCommands;
  if (e.key === 'ArrowDown') {
    e.preventDefault();
    activeIndex.value = (activeIndex.value + 1) % items.length;
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    activeIndex.value = (activeIndex.value - 1 + items.length) % items.length;
  } else if (e.key === 'Enter') {
    e.preventDefault();
    if (items[activeIndex.value]) {
      selectCommand(items[activeIndex.value]);
    }
  } else if (e.key === 'Escape') {
    emit('close');
  }
}

function onBackdropClick(e: MouseEvent) {
  if ((e.target as HTMLElement).classList.contains('palette-backdrop')) {
    emit('close');
  }
}

watch(query, () => {
  activeIndex.value = 0;
});

onMounted(() => {
  nextTick(() => inputRef.value?.focus());
  document.addEventListener('keydown', onKeydown);
});

onUnmounted(() => {
  document.removeEventListener('keydown', onKeydown);
});
</script>

<template>
  <div class="palette-backdrop" @click="onBackdropClick">
    <div class="palette-panel" role="dialog" aria-label="Bảng lệnh">
      <div class="palette-input-wrap">
        <el-icon :size="18"><Search /></el-icon>
        <input
          ref="inputRef"
          v-model="query"
          class="palette-input"
          placeholder="Tìm kiếm lệnh..."
        aria-label="Tìm lệnh"
      />
      </div>

      <div class="palette-results" v-if="visibleCommands.length > 0">
        <template v-for="group in groupedCommands()" :key="group.category">
          <p class="palette-category">{{ group.category }}</p>
          <button
            v-for="(cmd, idx) in group.items"
            :key="cmd.id"
            class="palette-item"
            :class="{ 'palette-item--active': visibleCommands.indexOf(cmd) === activeIndex }"
            @click="selectCommand(cmd)"
            @mouseenter="activeIndex = visibleCommands.indexOf(cmd)"
          >
            <el-icon :size="16"><component :is="cmd.icon" /></el-icon>
            <span class="palette-item-label">{{ cmd.label }}</span>
            <span v-if="cmd.shortcut" class="palette-item-shortcut">{{ cmd.shortcut }}</span>
          </button>
        </template>
      </div>

      <div class="palette-empty" v-else>
        <p>Không tìm thấy kết quả</p>
      </div>

      <div class="palette-footer">
        <span>↑↓ di chuyển · Enter chọn · Esc đóng</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.palette-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  z-index: var(--z-modal);
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding-top: 120px;
}

.palette-panel {
  width: 560px;
  max-width: calc(100vw - 32px);
  background: var(--color-surface-2);
  border-radius: 12px;
  box-shadow: var(--elevation-floating);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  max-height: 400px;
}

.palette-input-wrap {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 16px;
  height: 48px;
  border-bottom: 1px solid var(--color-border);
  color: var(--color-text-muted);
}

.palette-input {
  flex: 1;
  border: none;
  background: transparent;
  font-size: 15px;
  color: var(--color-text-primary);
  font-family: inherit;
  outline: none;
}

.palette-input::placeholder {
  color: var(--color-text-muted);
}

.palette-results {
  flex: 1;
  overflow-y: auto;
  padding: 8px 0;
}

.palette-category {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--color-text-muted);
  padding: 8px 16px 4px;
  margin: 0;
}

.palette-item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 10px 16px;
  border: none;
  background: transparent;
  color: var(--color-text-primary);
  font-size: 14px;
  font-family: inherit;
  cursor: pointer;
  text-align: left;
  transition: background var(--duration-fast) var(--ease-standard);
}

.palette-item:hover,
.palette-item--active {
  background: rgba(245, 230, 66, 0.15);
  color: #0D0D0D;
}

.palette-item-label {
  flex: 1;
}

.palette-item-shortcut {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--color-text-muted);
  padding: 1px 6px;
  border-radius: 4px;
  border: 1px solid var(--color-border);
  background: var(--color-canvas);
  line-height: 1.4;
}

.palette-empty {
  padding: 32px 16px;
  text-align: center;
  color: var(--color-text-muted);
  font-size: 14px;
}

.palette-footer {
  padding: 8px 16px;
  border-top: 1px solid var(--color-border);
  font-size: 11px;
  color: var(--color-text-muted);
}

@media (max-width: 640px) {
  .palette-backdrop {
    padding-top: 64px;
  }

  .palette-panel {
    max-height: calc(100dvh - 80px);
  }
}
</style>
