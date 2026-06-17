<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { CrmLead, LeadConvertModel } from '@/types/crm/lead';

const props = defineProps<{ modelValue: boolean; lead?: CrmLead | null; submitting?: boolean }>();
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: LeadConvertModel): void;
}>();

const form = reactive<LeadConvertModel>({ address: '', company: '', notes: '', source: 'CRM Lead' });

watch(() => props.modelValue, (open) => {
  if (open) {
    form.address = '';
    form.company = '';
    form.notes = '';
    form.source = 'CRM Lead';
  }
});
</script>

<template>
  <el-dialog :model-value="modelValue" title="Chuyển thành khách hàng" width="460px" @update:model-value="emit('update:modelValue', $event)">
    <div class="convert-summary">
      <strong>{{ lead?.fullName }}</strong>
      <span>{{ lead?.phone || lead?.email }}</span>
      <p>Lead sẽ được chuyển sang trạng thái Thành công. Phase này chỉ mô phỏng phía Lead, chưa tạo Customer UI.</p>
    </div>
    <el-form label-position="top">
      <el-form-item label="Địa chỉ">
        <el-input v-model="form.address" />
      </el-form-item>
      <el-form-item label="Công ty">
        <el-input v-model="form.company" />
      </el-form-item>
      <el-form-item label="Ghi chú">
        <el-input v-model="form.notes" type="textarea" :rows="3" />
      </el-form-item>
      <el-form-item label="Nguồn">
        <el-input v-model="form.source" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="emit('update:modelValue', false)">Hủy</el-button>
      <el-button type="primary" :loading="submitting" @click="emit('submit', { ...form })">Chuyển đổi</el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.convert-summary {
  background: var(--color-warning-bg);
  border: 1px solid var(--color-warning-border);
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  gap: 5px;
  margin-bottom: 14px;
  padding: 12px;
}

strong {
  color: var(--color-text-primary);
}

span,
p {
  color: var(--color-text-secondary);
  margin: 0;
}
</style>
