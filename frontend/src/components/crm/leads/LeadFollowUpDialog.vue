<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { CrmLead, LeadFollowUpModel } from '@/types/crm/lead';

const props = defineProps<{ modelValue: boolean; lead?: CrmLead | null; submitting?: boolean }>();
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: LeadFollowUpModel): void;
}>();

const form = reactive<LeadFollowUpModel>({ nextFollowUpAt: '', note: '' });

watch(() => props.modelValue, (open) => {
  if (open) {
    const date = props.lead?.nextFollowUpAt ? new Date(props.lead.nextFollowUpAt) : new Date(Date.now() + 24 * 60 * 60 * 1000);
    form.nextFollowUpAt = date.toISOString().slice(0, 16);
    form.note = '';
  }
});
</script>

<template>
  <el-dialog :model-value="modelValue" title="Đặt lịch follow-up" width="440px" @update:model-value="emit('update:modelValue', $event)">
    <el-form label-position="top">
      <el-form-item label="Thời gian chăm sóc" required>
        <el-date-picker v-model="form.nextFollowUpAt" type="datetime" format="DD/MM/YYYY HH:mm" value-format="YYYY-MM-DDTHH:mm:ss" />
      </el-form-item>
      <el-form-item label="Ghi chú">
        <el-input v-model="form.note" type="textarea" :rows="3" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="emit('update:modelValue', false)">Hủy</el-button>
      <el-button type="primary" :loading="submitting" @click="emit('submit', { ...form, nextFollowUpAt: new Date(form.nextFollowUpAt).toISOString() })">Lưu lịch</el-button>
    </template>
  </el-dialog>
</template>
