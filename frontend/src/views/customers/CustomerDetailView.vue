<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import CrmEmptyState from '@/components/crm/common/CrmEmptyState.vue';
import CustomerActivityTimeline from '@/components/crm/customers/CustomerActivityTimeline.vue';
import CustomerAssignDialog from '@/components/crm/customers/CustomerAssignDialog.vue';
import CustomerFormDrawer from '@/components/crm/customers/CustomerFormDrawer.vue';
import CustomerOverviewPanel from '@/components/crm/customers/CustomerOverviewPanel.vue';
import CustomerProfileHeader from '@/components/crm/customers/CustomerProfileHeader.vue';
import CustomerSourcePanel from '@/components/crm/customers/CustomerSourcePanel.vue';
import CustomerWorkflowPanel from '@/components/crm/customers/CustomerWorkflowPanel.vue';
import { useAuthStore } from '@/stores/useAuthStore';
import { useCustomerStore } from '@/stores/useCustomerStore';
import { useToastStore } from '@/stores/useToastStore';
import type { CustomerAssignmentModel, CustomerCreateModel } from '@/types/crm/customer';

const route = useRoute();
const router = useRouter();
const customerStore = useCustomerStore();
const toastStore = useToastStore();
const authStore = useAuthStore();

const editOpen = ref(false);
const assignOpen = ref(false);
const customerId = computed(() => String(route.params.id));
const customer = computed(() => customerStore.selectedCustomer);
const role = computed(() => authStore.user?.role ?? 'Viewer');
const canWrite = computed(() => ['Admin', 'Manager', 'Sales', 'Agent'].includes(role.value));
const canDelete = computed(() => ['Admin', 'Manager'].includes(role.value));

onMounted(async () => {
  await customerStore.fetchCustomerById(customerId.value);
  await customerStore.fetchActivities(customerId.value);
});

function openLead(leadId: string) {
  router.push({ name: 'lead-detail', params: { id: leadId } });
}

async function submitEdit(payload: CustomerCreateModel) {
  if (!customer.value) return;
  try {
    await customerStore.updateCustomer(customer.value.id, payload);
    await customerStore.fetchActivities(customer.value.id);
    toastStore.success('Đã cập nhật khách hàng');
    editOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể cập nhật', error instanceof Error ? error.message : undefined);
  }
}

async function submitAssign(payload: CustomerAssignmentModel) {
  if (!customer.value) return;
  try {
    await customerStore.assignCustomer(customer.value.id, payload);
    await customerStore.fetchActivities(customer.value.id);
    toastStore.success('Đã phân công khách hàng');
    assignOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể phân công', error instanceof Error ? error.message : undefined);
  }
}

async function deleteCustomer() {
  if (!customer.value) return;
  try {
    await ElMessageBox.confirm(`Xóa khách hàng ${customer.value.fullName}?`, 'Xác nhận xóa', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning'
    });
    await customerStore.deleteCustomer(customer.value.id);
    toastStore.success('Đã xóa khách hàng');
    router.push({ name: 'customer-list' });
  } catch {
    // User canceled.
  }
}
</script>

<template>
  <div class="page customer-detail-page">
    <el-breadcrumb separator="/">
      <el-breadcrumb-item :to="{ name: 'customer-list' }">Khách hàng</el-breadcrumb-item>
      <el-breadcrumb-item>Chi tiết khách hàng</el-breadcrumb-item>
    </el-breadcrumb>

    <el-skeleton v-if="customerStore.detailLoading" :rows="8" animated class="glass-card detail-loading" />

    <CrmEmptyState
      v-else-if="!customer"
      class="glass-card"
      title="Không tìm thấy khách hàng"
      description="Khách hàng có thể đã bị xóa khỏi mock database hoặc đường dẫn không hợp lệ."
    >
      <template #action>
        <el-button type="primary" @click="router.push({ name: 'customer-list' })">Quay lại danh sách</el-button>
      </template>
    </CrmEmptyState>

    <template v-else>
      <CustomerProfileHeader :customer="customer" :can-write="canWrite" @back="router.push({ name: 'customer-list' })" @edit="editOpen = true" />

      <div class="detail-grid">
        <div class="detail-main">
          <CustomerOverviewPanel :customer="customer" />
          <CustomerSourcePanel :customer="customer" @open-lead="openLead" />
          <CustomerActivityTimeline :activities="customerStore.activities" />
        </div>
        <aside class="detail-side">
          <CustomerWorkflowPanel
            :customer="customer"
            :can-write="canWrite"
            :can-delete="canDelete"
            @edit="editOpen = true"
            @assign="assignOpen = true"
            @open-lead="openLead"
            @delete="deleteCustomer"
          />
        </aside>
      </div>

      <CustomerFormDrawer v-model="editOpen" :customer="customer" :submitting="customerStore.submitting" @submit="submitEdit" />
      <CustomerAssignDialog v-model="assignOpen" :customer="customer" :submitting="customerStore.submitting" @submit="submitAssign" />
    </template>
  </div>
</template>

<style scoped>
.customer-detail-page {
  gap: 16px;
}

.detail-loading {
  padding: 24px;
}

.detail-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: minmax(0, 2fr) 360px;
}

.detail-main {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
}

.detail-side {
  position: sticky;
  top: 84px;
}

@media (max-width: 1100px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }

  .detail-side {
    position: static;
  }
}
</style>
