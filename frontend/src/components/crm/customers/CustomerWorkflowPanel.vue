<script setup lang="ts">
import { Delete, Edit, Link, UserFilled } from '@element-plus/icons-vue';
import type { CrmCustomerDetail } from '@/types/crm/customer';
import CrmSectionCard from '@/components/crm/common/CrmSectionCard.vue';

defineProps<{
  customer: CrmCustomerDetail;
  canWrite: boolean;
  canDelete: boolean;
}>();

defineEmits<{
  (e: 'edit'): void;
  (e: 'assign'): void;
  (e: 'openLead', leadId: string): void;
  (e: 'delete'): void;
}>();
</script>

<template>
  <CrmSectionCard title="Workflow" description="Customer chỉ hỗ trợ quản lý hồ sơ và phân công.">
    <div class="workflow">
      <el-button :icon="Edit" :disabled="!canWrite" @click="$emit('edit')">Chỉnh sửa Customer</el-button>
      <el-button :icon="UserFilled" :disabled="!canWrite" @click="$emit('assign')">Phân công phụ trách</el-button>
      <el-button v-if="customer.convertedFromLeadId" :icon="Link" @click="$emit('openLead', customer.convertedFromLeadId)">Mở Lead gốc</el-button>
      <el-button :icon="Delete" type="danger" plain :disabled="!canDelete" @click="$emit('delete')">Xóa Customer</el-button>
    </div>
  </CrmSectionCard>
</template>

<style scoped>
.workflow {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.workflow :deep(.el-button) {
  justify-content: flex-start;
  margin-left: 0;
  width: 100%;
}
</style>
