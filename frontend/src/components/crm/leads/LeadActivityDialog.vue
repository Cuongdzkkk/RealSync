<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { LeadActivityCreateModel } from '@/types/crm/lead';
import { USER_ACTIVITY_TYPES } from '@/types/crm/lead';
import { getActivityTypeLabel } from '@/utils/crm';

const props = defineProps<{ modelValue: boolean; submitting?: boolean }>();
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: LeadActivityCreateModel): void;
}>();

const form = reactive<LeadActivityCreateModel>({ activityType: 'Call', description: '' });

watch(() => props.modelValue, (open) => {
  if (open) {
    form.activityType = 'Call';
    form.description = '';
  }
});
</script>

<template>
  <el-dialog :model-value="modelValue" title="Thêm hoạt động" width="440px" @update:model-value="emit('update:modelValue', $event)">
    <el-form label-position="top">
      <el-form-item label="Loại hoạt động">
        <el-select v-model="form.activityType">
          <el-option v-for="type in USER_ACTIVITY_TYPES" :key="type" :label="getActivityTypeLabel(type)" :value="type" />
        </el-select>
      </el-form-item>
      <el-form-item label="Mô tả">
        <el-input v-model="form.description" type="textarea" :rows="4" placeholder="Ghi chú nội dung chăm sóc..." />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="emit('update:modelValue', false)">Hủy</el-button>
      <el-button type="primary" :loading="submitting" @click="emit('submit', { ...form })">Lưu hoạt động</el-button>
    </template>
  </el-dialog>
</template>
