<script setup lang="ts">
import type { AiClassificationJob } from '@/types/ai';
import { formatDate } from '@/utils/format';
import StatusBadge from '@/components/common/StatusBadge.vue';

defineProps<{
  jobs: AiClassificationJob[];
}>();

const statusVariant = (status: AiClassificationJob['status']) => {
  if (status === 'completed') return 'success';
  if (status === 'failed') return 'danger';
  if (status === 'review') return 'warning';
  return 'ai';
};
</script>

<template>
  <el-table :data="jobs">
    <el-table-column prop="target" label="Đối tượng" min-width="260" />
    <el-table-column prop="type" label="Loại" width="120" />
    <el-table-column label="Trạng thái" width="150">
      <template #default="{ row }">
        <StatusBadge :label="row.status" :variant="statusVariant(row.status)" :pulse="row.status === 'processing'" />
      </template>
    </el-table-column>
    <el-table-column label="Độ tin cậy" width="130">
      <template #default="{ row }">
        <span class="numeric">{{ row.confidence }}%</span>
      </template>
    </el-table-column>
    <el-table-column prop="result" label="Kết quả" min-width="240" />
    <el-table-column label="Ngày tạo" width="130">
      <template #default="{ row }">
        {{ formatDate(row.createdAt) }}
      </template>
    </el-table-column>
  </el-table>
</template>
