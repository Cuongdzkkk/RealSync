<script setup lang="ts">
import { computed } from 'vue';
import { Calendar, ChatLineRound, CircleCheck, Delete, Edit, Refresh, UserFilled } from '@element-plus/icons-vue';
import { useAuthStore } from '@/stores/useAuthStore';
import type { CrmLead, LeadStatus } from '@/types/crm/lead';
import { LEAD_STATUSES } from '@/types/crm/lead';
import { getLeadStatusLabel } from '@/utils/crm';
import CrmSectionCard from '@/components/crm/common/CrmSectionCard.vue';

defineProps<{ lead: CrmLead }>();
defineEmits<{
  (e: 'status', status: LeadStatus): void;
  (e: 'assign'): void;
  (e: 'activity'): void;
  (e: 'followUp'): void;
  (e: 'convert'): void;
  (e: 'edit'): void;
  (e: 'delete'): void;
}>();

const authStore = useAuthStore();
const role = computed(() => authStore.user?.role ?? 'Sales');
const canAssign = computed(() => role.value === 'Admin' || role.value === 'Manager');
const canConvert = computed(() => role.value !== 'Viewer');
</script>

<template>
  <CrmSectionCard title="Workflow" description="Các thao tác chăm sóc Lead bằng mock data.">
    <div class="workflow">
      <el-select :model-value="lead.status" @change="$emit('status', $event as LeadStatus)">
        <el-option v-for="status in LEAD_STATUSES" :key="status" :label="getLeadStatusLabel(status)" :value="status" />
      </el-select>
      <el-button :icon="UserFilled" :disabled="!canAssign" @click="$emit('assign')">Phân công Lead</el-button>
      <el-button :icon="ChatLineRound" @click="$emit('activity')">Thêm hoạt động</el-button>
      <el-button :icon="Calendar" @click="$emit('followUp')">Đặt follow-up</el-button>
      <el-button :icon="CircleCheck" :disabled="!canConvert || !!lead.convertedAt" @click="$emit('convert')">Chuyển thành khách hàng</el-button>
      <el-button :icon="Edit" @click="$emit('edit')">Chỉnh sửa Lead</el-button>
      <el-button :icon="Delete" type="danger" plain @click="$emit('delete')">Xóa Lead</el-button>
      <div class="workflow__note">
        <el-icon><Refresh /></el-icon>
        <span>Role UI gate mock: {{ role }}</span>
      </div>
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

.workflow__note {
  align-items: center;
  background: var(--color-ai-bg);
  border: 1px solid var(--color-ai-border);
  border-radius: 9px;
  color: var(--color-ai);
  display: flex;
  font-size: 12px;
  gap: 8px;
  padding: 10px;
}
</style>
