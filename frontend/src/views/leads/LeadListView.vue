<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import { Plus, Tickets, Grid } from '@element-plus/icons-vue';
import CrmPageHeader from '@/components/crm/common/CrmPageHeader.vue';
import LeadAssignDialog from '@/components/crm/leads/LeadAssignDialog.vue';
import LeadFilters from '@/components/crm/leads/LeadFilters.vue';
import LeadFollowUpDialog from '@/components/crm/leads/LeadFollowUpDialog.vue';
import LeadFormDrawer from '@/components/crm/leads/LeadFormDrawer.vue';
import LeadKanban from '@/components/crm/leads/LeadKanban.vue';
import LeadStats from '@/components/crm/leads/LeadStats.vue';
import LeadTable from '@/components/crm/leads/LeadTable.vue';
import { useLeadStore } from '@/stores/useLeadStore';
import { useToastStore } from '@/stores/useToastStore';
import type { CrmLead, LeadAssignModel, LeadCreateModel, LeadFollowUpModel, LeadStatus } from '@/types/crm/lead';

const leadStore = useLeadStore();
const toastStore = useToastStore();
const router = useRouter();
const route = useRoute();

const formOpen = ref(false);
const assignOpen = ref(false);
const followUpOpen = ref(false);
const editingLead = ref<CrmLead | null>(null);
const activeLead = ref<CrmLead | null>(null);

const viewIcon = computed(() => (leadStore.viewMode === 'table' ? Grid : Tickets));

onMounted(async () => {
  await leadStore.fetchLeads();
  if (route.query.action === 'create') openCreateDrawer();
});

watch(
  () => route.query.action,
  (action) => {
    if (action === 'create') openCreateDrawer();
  }
);

watch(
  () => leadStore.query,
  () => leadStore.fetchLeads(),
  { deep: true }
);

function openCreateDrawer() {
  editingLead.value = null;
  formOpen.value = true;
  if (route.query.action) {
    router.replace({ name: 'lead-list', query: { ...route.query, action: undefined } });
  }
}

function openEditDrawer(lead: CrmLead) {
  editingLead.value = lead;
  formOpen.value = true;
}

function openLead(lead: CrmLead) {
  router.push({ name: 'lead-detail', params: { id: lead.id } });
}

function openAssign(lead: CrmLead) {
  activeLead.value = lead;
  assignOpen.value = true;
}

function openFollowUp(lead: CrmLead) {
  activeLead.value = lead;
  followUpOpen.value = true;
}

function validateLead(payload: LeadCreateModel) {
  if (!payload.fullName.trim()) throw new Error('Vui lòng nhập họ tên Lead.');
  if (!payload.phone && !payload.email) throw new Error('Vui lòng nhập số điện thoại hoặc email.');
  if (payload.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(payload.email)) throw new Error('Email không hợp lệ.');
  if (payload.phone && !/^[0-9+()\s.-]{8,20}$/.test(payload.phone)) throw new Error('Số điện thoại không hợp lệ.');
  if ((payload.score ?? 0) < 0 || (payload.score ?? 0) > 100) throw new Error('Score phải từ 0 đến 100.');
  if (payload.budget !== null && payload.budget !== undefined && payload.budget < 0) throw new Error('Ngân sách phải lớn hơn hoặc bằng 0.');
}

async function submitLead(payload: LeadCreateModel) {
  try {
    validateLead(payload);
    if (editingLead.value) {
      await leadStore.updateLead(editingLead.value.id, payload);
      toastStore.success('Đã cập nhật Lead', payload.fullName);
    } else {
      await leadStore.createLead(payload);
      toastStore.success('Đã thêm Lead', payload.fullName);
    }
    formOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể lưu Lead', error instanceof Error ? error.message : undefined);
  }
}

async function submitAssign(payload: LeadAssignModel) {
  if (!activeLead.value) return;
  try {
    await leadStore.assignLead(activeLead.value.id, payload);
    toastStore.success('Đã phân công Lead');
    assignOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể phân công', error instanceof Error ? error.message : undefined);
  }
}

async function submitFollowUp(payload: LeadFollowUpModel) {
  if (!activeLead.value) return;
  try {
    await leadStore.setFollowUp(activeLead.value.id, payload);
    toastStore.success('Đã đặt lịch follow-up');
    followUpOpen.value = false;
  } catch (error) {
    toastStore.error('Không thể đặt lịch', error instanceof Error ? error.message : undefined);
  }
}

async function updateStatus(lead: CrmLead, status: LeadStatus) {
  try {
    await leadStore.updateStatus(lead.id, { status, note: `Cập nhật trạng thái từ Kanban/List.` });
    toastStore.success('Đã cập nhật trạng thái');
  } catch (error) {
    toastStore.error('Không thể cập nhật trạng thái', error instanceof Error ? error.message : undefined);
  }
}

async function deleteLead(lead: CrmLead) {
  try {
    await ElMessageBox.confirm(`Xóa Lead ${lead.fullName}?`, 'Xác nhận xóa', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning'
    });
    await leadStore.deleteLead(lead.id);
    toastStore.success('Đã xóa Lead', lead.fullName);
  } catch {
    // User canceled.
  }
}
</script>

<template>
  <div class="page crm-lead-list">
    <CrmPageHeader
      title="Lead tiềm năng"
      subtitle="Quản lý khách hàng tiềm năng, lịch chăm sóc và tiến trình chuyển đổi."
    >
      <template #action>
        <el-button type="primary" :icon="Plus" @click="openCreateDrawer">Thêm Lead</el-button>
      </template>
    </CrmPageHeader>

    <LeadStats
      :total="leadStore.stats.total"
      :hot="leadStore.stats.hot"
      :overdue="leadStore.stats.overdue"
      :success-rate="leadStore.stats.successRate"
      :loading="leadStore.loading"
    />

    <LeadFilters :query="leadStore.query" @change="leadStore.setQuery" @reset="leadStore.resetFilters" />

    <div class="workspace-toolbar glass-card">
      <div>
        <strong>{{ leadStore.pagination.totalCount }} Lead</strong>
        <span>Mock-first workspace, chưa gọi API backend.</span>
      </div>
      <el-segmented
        :model-value="leadStore.viewMode"
        :options="[
          { label: 'Bảng', value: 'table' },
          { label: 'Kanban', value: 'kanban' }
        ]"
        @change="(value: string | number | boolean) => leadStore.setViewMode(value as 'table' | 'kanban')"
      />
      <el-icon class="workspace-toolbar__icon"><component :is="viewIcon" /></el-icon>
    </div>

    <LeadTable
      v-if="leadStore.viewMode === 'table'"
      :leads="leadStore.items"
      :loading="leadStore.loading"
      @open="openLead"
      @edit="openEditDrawer"
      @assign="openAssign"
      @follow-up="openFollowUp"
      @delete="deleteLead"
    />
    <LeadKanban
      v-else
      :groups="leadStore.kanbanItems"
      @open="openLead"
      @status="updateStatus"
    />

    <div class="pagination-row glass-card">
      <el-pagination
        background
        layout="prev, pager, next, sizes, total"
        :current-page="leadStore.pagination.page"
        :page-size="leadStore.pagination.pageSize"
        :total="leadStore.pagination.totalCount"
        :page-sizes="[6, 10, 20, 50]"
        @current-change="(page: number) => leadStore.setQuery({ page })"
        @size-change="(pageSize: number) => leadStore.setQuery({ pageSize, page: 1 })"
      />
    </div>

    <LeadFormDrawer
      v-model="formOpen"
      :lead="editingLead"
      :submitting="leadStore.submitting"
      @submit="submitLead"
    />
    <LeadAssignDialog
      v-model="assignOpen"
      :lead="activeLead"
      :submitting="leadStore.submitting"
      @submit="submitAssign"
    />
    <LeadFollowUpDialog
      v-model="followUpOpen"
      :lead="activeLead"
      :submitting="leadStore.submitting"
      @submit="submitFollowUp"
    />
  </div>
</template>

<style scoped>
.crm-lead-list {
  gap: 16px;
}

.workspace-toolbar,
.pagination-row {
  align-items: center;
  display: flex;
  gap: 12px;
  justify-content: space-between;
  padding: 12px 14px;
}

.workspace-toolbar strong {
  color: var(--color-text-primary);
}

.workspace-toolbar span {
  color: var(--color-text-muted);
  display: block;
  font-size: 12px;
  margin-top: 3px;
}

.workspace-toolbar__icon {
  color: var(--color-text-muted);
}

@media (max-width: 768px) {
  .workspace-toolbar,
  .pagination-row {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
