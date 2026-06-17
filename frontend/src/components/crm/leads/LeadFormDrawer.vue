<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import type { CrmLead, LeadCreateModel } from '@/types/crm/lead';
import { LEAD_PRIORITIES, LEAD_SOURCE_CHANNELS, LEAD_STATUSES } from '@/types/crm/lead';
import { getLeadPriorityLabel, getLeadStatusLabel } from '@/utils/crm';
import { mockCrmUsers } from '@/mocks/crm/users';

const props = defineProps<{
  modelValue: boolean;
  lead?: CrmLead | null;
  submitting?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: LeadCreateModel): void;
}>();

const activeUsers = computed(() => mockCrmUsers.filter((user) => user.isActive));
const title = computed(() => (props.lead ? 'Cập nhật Lead' : 'Thêm Lead'));

const form = reactive<LeadCreateModel>({
  fullName: '',
  phone: '',
  email: '',
  status: 'New',
  priority: 'Normal',
  score: 40,
  requirements: '',
  budget: null,
  preferredArea: '',
  preferredType: '',
  interestedPropertyId: null,
  interestedPropertyTitle: '',
  sourceChannel: 'Website',
  assignedToId: null
});

function syncForm() {
  const lead = props.lead;
  form.fullName = lead?.fullName ?? '';
  form.phone = lead?.phone ?? '';
  form.email = lead?.email ?? '';
  form.status = lead?.status ?? 'New';
  form.priority = lead?.priority ?? 'Normal';
  form.score = lead?.score ?? 40;
  form.requirements = lead?.requirements ?? '';
  form.budget = lead?.budget ?? null;
  form.preferredArea = lead?.preferredArea ?? '';
  form.preferredType = lead?.preferredType ?? '';
  form.interestedPropertyId = lead?.interestedPropertyId ?? null;
  form.interestedPropertyTitle = lead?.interestedPropertyTitle ?? '';
  form.sourceChannel = lead?.sourceChannel ?? 'Website';
  form.assignedToId = lead?.assignedToId ?? null;
}

watch(() => props.modelValue, (open) => {
  if (open) syncForm();
});

function submit() {
  emit('submit', { ...form });
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    :title="title"
    custom-class="crm-lead-drawer"
    size="620px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <el-form label-position="top" class="lead-form" @submit.prevent="submit">
      <section>
        <h3>Thông tin liên hệ</h3>
        <el-form-item label="Họ tên" required>
          <el-input v-model="form.fullName" placeholder="Nguyễn Văn A" />
        </el-form-item>
        <div class="form-grid">
          <el-form-item label="Số điện thoại">
            <el-input v-model="form.phone" placeholder="090..." />
          </el-form-item>
          <el-form-item label="Email">
            <el-input v-model="form.email" placeholder="lead@example.com" />
          </el-form-item>
        </div>
      </section>

      <section>
        <h3>Phân loại</h3>
        <div class="form-grid form-grid--three">
          <el-form-item label="Trạng thái">
            <el-select v-model="form.status">
              <el-option v-for="status in LEAD_STATUSES" :key="status" :label="getLeadStatusLabel(status)" :value="status" />
            </el-select>
          </el-form-item>
          <el-form-item label="Ưu tiên">
            <el-select v-model="form.priority">
              <el-option v-for="priority in LEAD_PRIORITIES" :key="priority" :label="getLeadPriorityLabel(priority)" :value="priority" />
            </el-select>
          </el-form-item>
          <el-form-item label="Score">
            <el-input-number v-model="form.score" :min="0" :max="100" controls-position="right" />
          </el-form-item>
        </div>
      </section>

      <section>
        <h3>Nhu cầu</h3>
        <el-form-item label="Nhu cầu">
          <el-input v-model="form.requirements" type="textarea" :rows="3" placeholder="Căn hộ 2PN Thủ Thiêm..." />
        </el-form-item>
        <div class="form-grid">
          <el-form-item label="Ngân sách">
            <el-input-number v-model="form.budget" :min="0" :step="100000000" controls-position="right" />
          </el-form-item>
          <el-form-item label="Khu vực">
            <el-input v-model="form.preferredArea" />
          </el-form-item>
          <el-form-item label="Loại BĐS">
            <el-input v-model="form.preferredType" />
          </el-form-item>
          <el-form-item label="Sản phẩm mock">
            <el-input v-model="form.interestedPropertyTitle" />
          </el-form-item>
        </div>
      </section>

      <section>
        <h3>Nguồn và phân công</h3>
        <div class="form-grid">
          <el-form-item label="Nguồn">
            <el-select v-model="form.sourceChannel">
              <el-option v-for="source in LEAD_SOURCE_CHANNELS" :key="source" :label="source" :value="source" />
            </el-select>
          </el-form-item>
          <el-form-item label="Người phụ trách">
            <el-select v-model="form.assignedToId" clearable placeholder="Chưa phân công">
              <el-option v-for="user in activeUsers" :key="user.id" :label="user.fullName" :value="user.id" />
            </el-select>
          </el-form-item>
        </div>
      </section>
    </el-form>

    <template #footer>
      <el-button @click="emit('update:modelValue', false)">Hủy</el-button>
      <el-button type="primary" :loading="submitting" @click="submit">Lưu Lead</el-button>
    </template>
  </el-drawer>
</template>

<style scoped>
.lead-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

section {
  background: var(--color-surface-glass);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 14px;
}

h3 {
  color: var(--color-text-primary);
  font-size: 13px;
  margin: 0 0 12px;
}

.form-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.form-grid--three {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

@media (max-width: 680px) {
  .form-grid,
  .form-grid--three {
    grid-template-columns: 1fr;
  }
}
</style>
