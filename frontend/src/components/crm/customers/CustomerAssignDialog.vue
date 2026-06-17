<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { CrmCustomerDetail, CustomerAssignmentModel } from '@/types/crm/customer';
import { mockCrmUsers } from '@/mocks/crm/users';

const props = defineProps<{ modelValue: boolean; customer?: CrmCustomerDetail | null; submitting?: boolean }>();
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: CustomerAssignmentModel): void;
}>();

const activeUsers = mockCrmUsers.filter((user) => user.isActive);
const form = reactive<CustomerAssignmentModel>({ assignedToId: '', note: '' });

watch(() => props.modelValue, (open) => {
  if (open) {
    form.assignedToId = props.customer?.assignedToId ?? '';
    form.note = '';
  }
});
</script>

<template>
  <el-dialog :model-value="modelValue" title="Phân công khách hàng" width="420px" @update:model-value="emit('update:modelValue', $event)">
    <el-form label-position="top">
      <el-form-item label="Người phụ trách" required>
        <el-select v-model="form.assignedToId">
          <el-option v-for="user in activeUsers" :key="user.id" :label="`${user.fullName} · ${user.role}`" :value="user.id" />
        </el-select>
      </el-form-item>
      <el-form-item label="Ghi chú">
        <el-input v-model="form.note" type="textarea" :rows="3" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="emit('update:modelValue', false)">Hủy</el-button>
      <el-button type="primary" :loading="submitting" :disabled="!form.assignedToId" @click="emit('submit', { ...form })">Phân công</el-button>
    </template>
  </el-dialog>
</template>
