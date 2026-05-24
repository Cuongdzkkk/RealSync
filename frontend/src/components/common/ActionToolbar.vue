<script setup lang="ts">
import { Filter, Refresh, Search } from '@element-plus/icons-vue';

const keyword = defineModel<string>('keyword', { default: '' });

defineProps<{
  placeholder?: string;
}>();
</script>

<template>
  <div class="action-toolbar">
    <el-input v-model="keyword" :prefix-icon="Search" :placeholder="placeholder ?? 'Tìm kiếm...'" clearable />
    <div class="action-toolbar__actions">
      <el-button :icon="Filter">Lọc</el-button>
      <el-button :icon="Refresh">Làm mới</el-button>
      <slot />
    </div>
  </div>
</template>

<style scoped>
.action-toolbar {
  align-items: center;
  display: grid;
  gap: 12px;
  grid-template-columns: minmax(260px, 1fr) auto;
}

.action-toolbar__actions {
  align-items: center;
  display: flex;
  gap: 8px;
}

@media (max-width: 760px) {
  .action-toolbar {
    grid-template-columns: 1fr;
  }

  .action-toolbar__actions {
    flex-wrap: wrap;
  }
}
</style>
