<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import { Plus } from '@element-plus/icons-vue';
import CrmPageHeader from '@/components/crm/common/CrmPageHeader.vue';
import CustomerAssignDialog from '@/components/crm/customers/CustomerAssignDialog.vue';
import CustomerFilters from '@/components/crm/customers/CustomerFilters.vue';
import CustomerFormDrawer from '@/components/crm/customers/CustomerFormDrawer.vue';
import CustomerStats from '@/components/crm/customers/CustomerStats.vue';
import CustomerTable from '@/components/crm/customers/CustomerTable.vue';
import { useAuthStore } from '@/stores/useAuthStore';
import { useCustomerStore } from '@/stores/useCustomerStore';
import { useToastStore } from '@/stores/useToastStore';
import type { CrmCustomerDetail, CustomerAssignmentModel, CustomerCreateModel } from '@/types/crm/customer';

const customerStore = useCustomerStore();
const toastStore = useToastStore();
const authStore = useAuthStore();
const router = useRouter();

const formOpen = ref(false);
const assignOpen = ref(false);
const editingCustomer = ref<CrmCustomerDetail | null>(null);
const activeCustomer = ref<CrmCustomerDetail | null>(null);

const role = computed(() => authStore.user?.role ?? 'Viewer');
const canWrite = computed(() => ['Admin', 'Manager', 'Sales', 'Agent'].includes(role.value));
const canDelete = computed(() => ['Admin', 'Manager'].includes(role.value));

onMounted(() => customerStore.fetchCustomers());

watch(
  () => customerStore.query,
  () => customerStore.fetchCustomers(),
  { deep: true }
);

function openCreateDrawer() {
  editingCustomer.value = null;
  formOpen.value = true;
}

function openEditDrawer(customer: CrmCustomerDetail) {
  editingCustomer.value = customer;
  formOpen.value = true;
}

function openAssign(customer: CrmCustomerDetail) {
  activeCustomer.value = customer;
  assignOpen.value = true;
}

function openCustomer(customer: CrmCustomerDetail) {
  router.push({ name: 'customer-detail', params: { id: customer.id } });
}

function openLead(leadId: string) {
  router.push({ name: 'lead-detail', params: { id: leadId } });
}

function validateCustomer(payload: CustomerCreateModel) {
  if (!payload.fullName.trim()) throw new Error('Vui lòng nhập họ tên khách hàng.');
  if (!payload.phone && !payload.email) throw new Error('Vui lòng nhập số điện thoại hoặc email.');
  if (payload.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(payload.email)) throw new Error('Email không hợp lệ.');
}

async function submitCustomer(payload: CustomerCreateModel) {
  try {
    validateCustomer(payload);
    if (editingCustomer.value) {
      await customerStore.updateCustomer(editingCustomer.value.id, payload);
      toastStore.success('Đã cập nhật khách hàng', payload.fullName);
    } else {
      await customerStore.createCustomer(payload);
      toastStore.success('Đã thêm khách hàng', payload.fullName);
    }
    formOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể lưu khách hàng', error instanceof Error ? error.message : undefined);
  }
}

async function submitAssign(payload: CustomerAssignmentModel) {
  if (!activeCustomer.value) return;
  try {
    await customerStore.assignCustomer(activeCustomer.value.id, payload);
    toastStore.success('Đã phân công khách hàng');
    assignOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể phân công', error instanceof Error ? error.message : undefined);
  }
}

async function deleteCustomer(customer: CrmCustomerDetail) {
  try {
    await ElMessageBox.confirm(`Xóa khách hàng ${customer.fullName}?`, 'Xác nhận xóa', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning'
    });
    await customerStore.deleteCustomer(customer.id);
    toastStore.success('Đã xóa khách hàng', customer.fullName);
  } catch {
    // User canceled.
  }
}
</script>

<template>
  <div class="page customer-list-page">
    <CrmPageHeader title="Khách hàng" subtitle="Quản lý hồ sơ khách hàng chính thức và nguồn chuyển đổi.">
      <template #action>
        <el-button v-if="canWrite" type="primary" :icon="Plus" @click="openCreateDrawer">Thêm khách hàng</el-button>
      </template>
    </CrmPageHeader>

    <CustomerStats
      :total="customerStore.stats.total"
      :converted="customerStore.stats.converted"
      :direct="customerStore.stats.direct"
      :unassigned="customerStore.stats.unassigned"
      :loading="customerStore.loading"
    />

    <CustomerFilters :query="customerStore.query" @change="customerStore.setQuery" @reset="customerStore.resetFilters" />

    <CustomerTable
      :customers="customerStore.items"
      :loading="customerStore.loading"
      :can-write="canWrite"
      :can-delete="canDelete"
      @open="openCustomer"
      @edit="openEditDrawer"
      @assign="openAssign"
      @delete="deleteCustomer"
      @open-lead="openLead"
    />

    <div class="pagination-row glass-card">
      <el-pagination
        background
        layout="prev, pager, next, sizes, total"
        :current-page="customerStore.pagination.page"
        :page-size="customerStore.pagination.pageSize"
        :total="customerStore.pagination.totalCount"
        :page-sizes="[6, 10, 20, 50]"
        @current-change="(page: number) => customerStore.setQuery({ page })"
        @size-change="(pageSize: number) => customerStore.setQuery({ pageSize, page: 1 })"
      />
    </div>

    <CustomerFormDrawer
      v-model="formOpen"
      :customer="editingCustomer"
      :submitting="customerStore.submitting"
      @submit="submitCustomer"
    />
    <CustomerAssignDialog
      v-model="assignOpen"
      :customer="activeCustomer"
      :submitting="customerStore.submitting"
      @submit="submitAssign"
    />
  </div>
</template>

<style scoped>
.customer-list-page {
  gap: 16px;
}

.pagination-row {
  align-items: center;
  display: flex;
  gap: 12px;
  justify-content: space-between;
  padding: 12px 14px;
}

@media (max-width: 768px) {
  .pagination-row {
    align-items: flex-start;
    flex-direction: column;
    justify-content: flex-start;
    overflow-x: auto;
  }
}
</style>
