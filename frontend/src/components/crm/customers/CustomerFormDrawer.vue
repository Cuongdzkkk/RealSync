<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import { CUSTOMER_SOURCES, type CrmCustomerDetail, type CustomerCreateModel } from '@/types/crm/customer';
import { mockCrmUsers } from '@/mocks/crm/users';

const props = defineProps<{
  modelValue: boolean;
  customer?: CrmCustomerDetail | null;
  submitting?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', value: CustomerCreateModel): void;
}>();

const title = computed(() => (props.customer ? 'Cập nhật khách hàng' : 'Thêm khách hàng'));
const activeUsers = mockCrmUsers.filter((user) => user.isActive);
const form = reactive<CustomerCreateModel>({
  fullName: '',
  phone: '',
  email: '',
  address: '',
  company: '',
  notes: '',
  source: 'Website',
  assignedToId: null
});

function syncForm() {
  const customer = props.customer;
  form.fullName = customer?.fullName ?? '';
  form.phone = customer?.phone ?? '';
  form.email = customer?.email ?? '';
  form.address = customer?.address ?? '';
  form.company = customer?.company ?? '';
  form.notes = customer?.notes ?? '';
  form.source = customer?.source ?? 'Website';
  form.assignedToId = customer?.assignedToId ?? null;
}

watch(() => props.modelValue, (open) => {
  if (open) syncForm();
});
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    :title="title"
    size="620px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <el-form label-position="top" class="customer-form" @submit.prevent="emit('submit', { ...form })">
      <section>
        <h3>Thông tin cơ bản</h3>
        <el-form-item label="Họ tên" required>
          <el-input v-model="form.fullName" maxlength="200" show-word-limit />
        </el-form-item>
        <div class="form-grid">
          <el-form-item label="Số điện thoại">
            <el-input v-model="form.phone" maxlength="20" />
          </el-form-item>
          <el-form-item label="Email">
            <el-input v-model="form.email" maxlength="200" />
          </el-form-item>
        </div>
      </section>

      <section>
        <h3>Thông tin bổ sung</h3>
        <el-form-item label="Địa chỉ">
          <el-input v-model="form.address" maxlength="500" show-word-limit />
        </el-form-item>
        <el-form-item label="Công ty">
          <el-input v-model="form.company" maxlength="200" show-word-limit />
        </el-form-item>
        <el-form-item label="Ghi chú">
          <el-input v-model="form.notes" type="textarea" :rows="4" maxlength="2000" show-word-limit />
        </el-form-item>
      </section>

      <section>
        <h3>Nguồn và phân công</h3>
        <div class="form-grid">
          <el-form-item label="Nguồn">
            <el-select v-model="form.source">
              <el-option v-for="source in CUSTOMER_SOURCES" :key="source" :label="source" :value="source" />
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
      <el-button type="primary" :loading="submitting" @click="emit('submit', { ...form })">Lưu khách hàng</el-button>
    </template>
  </el-drawer>
</template>

<style scoped>
.customer-form {
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

@media (max-width: 680px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
