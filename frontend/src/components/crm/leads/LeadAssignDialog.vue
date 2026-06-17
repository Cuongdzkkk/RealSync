<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { CrmLead, LeadAssignModel } from '@/types/crm/lead';
import { mockCrmUsers } from '@/mocks/crm/users';

const props = defineProps<{ modelValue: boolean; lead?: CrmLead | null; submitting?: boolean }>();
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: LeadAssignModel): void;
}>();

const form = reactive<LeadAssignModel>({ assignedToId: '', note: '' });
const activeUsers = mockCrmUsers.filter((user) => user.isActive);

watch(() => props.modelValue, (open) => {
  if (open) {
    form.assignedToId = props.lead?.assignedToId ?? '';
    form.note = '';
  }
});
</script>

<template>
  <el-dialog :model-value="modelValue" title="Phân công Lead" width="420px" @update:model-value="emit('update:modelValue', $event)">
    <el-form label-position="top">
      <el-form-item label="Người phụ trách" required>
        <el-select v-model="form.assignedToId" placeholder="Chọn nhân viên active">
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
