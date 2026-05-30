<script setup lang="ts">
import type { AiContentItem } from '@/types/ai';
import { formatDate } from '@/utils/format';
import StatusBadge from '@/components/common/StatusBadge.vue';

defineProps<{
  items: AiContentItem[];
}>();

const statusVariant = (status: AiContentItem['status']) => {
  if (status === 'published' || status === 'approved') return 'success';
  if (status === 'review') return 'warning';
  return 'info';
};
</script>

<template>
  <el-table :data="items">
    <el-table-column prop="title" label="Nội dung" min-width="260" />
    <el-table-column prop="channel" label="Kênh" width="120" />
    <el-table-column label="Trạng thái" width="140">
      <template #default="{ row }">
        <StatusBadge :label="row.status" :variant="statusVariant(row.status)" />
      </template>
    </el-table-column>
    <el-table-column prop="owner" label="Phụ trách" width="130" />
    <el-table-column label="Cập nhật" width="130">
      <template #default="{ row }">
        {{ formatDate(row.updatedAt) }}
      </template>
    </el-table-column>
  </el-table>
</template>
