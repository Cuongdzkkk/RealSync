<script setup lang="ts">
import { ArrowLeft, Edit } from '@element-plus/icons-vue';
import { formatDateTime, getInitials } from '@/utils/crm';
import type { CrmCustomerDetail } from '@/types/crm/customer';
import CustomerSourceBadge from './CustomerSourceBadge.vue';

defineProps<{ customer: CrmCustomerDetail; canWrite: boolean }>();
defineEmits<{
  (e: 'back'): void;
  (e: 'edit'): void;
}>();
</script>

<template>
  <section class="profile glass-card">
    <button class="icon-btn" aria-label="Quay lại danh sách khách hàng" @click="$emit('back')">
      <el-icon><ArrowLeft /></el-icon>
    </button>
    <div class="avatar">{{ getInitials(customer.fullName) }}</div>
    <div class="profile-main">
      <div class="profile-title">
        <h1>{{ customer.fullName }}</h1>
        <CustomerSourceBadge :source="customer.source" :converted="!!customer.convertedFromLeadId" />
      </div>
      <div class="profile-meta">
        <span>{{ customer.phone || 'Chưa có SĐT' }}</span>
        <span>{{ customer.email || 'Chưa có email' }}</span>
        <span>{{ customer.company || 'Cá nhân' }}</span>
        <span>{{ customer.assignedToName || 'Chưa phân công' }}</span>
        <span>Tạo {{ formatDateTime(customer.createdAt) }}</span>
      </div>
    </div>
    <el-button v-if="canWrite" :icon="Edit" @click="$emit('edit')">Chỉnh sửa</el-button>
  </section>
</template>

<style scoped>
.profile {
  align-items: center;
  display: flex;
  gap: 14px;
  padding: 16px;
}

.icon-btn {
  align-items: center;
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  color: var(--color-text-secondary);
  cursor: pointer;
  display: flex;
  height: 34px;
  justify-content: center;
  width: 34px;
}

.avatar {
  align-items: center;
  background: var(--color-yellow);
  border-radius: 12px;
  color: var(--color-yellow-text);
  display: flex;
  flex-shrink: 0;
  font-size: 16px;
  font-weight: 800;
  height: 50px;
  justify-content: center;
  width: 50px;
}

.profile-main {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 9px;
  min-width: 0;
}

.profile-title,
.profile-meta {
  align-items: center;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

h1 {
  color: var(--color-text-primary);
  font-size: 20px;
  margin: 0 8px 0 0;
}

.profile-meta {
  color: var(--color-text-secondary);
  font-size: 12px;
}

@media (max-width: 768px) {
  .profile {
    align-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
