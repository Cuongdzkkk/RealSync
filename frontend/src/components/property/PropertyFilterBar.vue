<script setup lang="ts">
import { Search } from '@element-plus/icons-vue';

const keyword = defineModel<string>('keyword', { default: '' });
const status = defineModel<string>('status', { default: '' });
</script>

<template>
  <div class="filter-bar">
    <el-input v-model="keyword" :prefix-icon="Search" placeholder="Tìm theo tiêu đề, địa chỉ..." clearable />
    <el-select v-model="status" placeholder="Trạng thái" clearable>
      <el-option label="Draft" value="draft" />
      <el-option label="Verified" value="verified" />
      <el-option label="Published" value="published" />
      <el-option label="Expired" value="expired" />
    </el-select>
  </div>
</template>

<style scoped>
.filter-bar {
  display: grid;
  gap: 12px;
  grid-template-columns: minmax(240px, 1fr) 180px;
}

@media (max-width: 640px) {
  .filter-bar {
    grid-template-columns: 1fr;
  }
}
</style>
