import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { crmLeadMockService } from '@/services/mock/crmLeadMockService';
import { mockLeads } from '@/mocks/crm/leads';
import { getFollowUpState, getLeadTemperature } from '@/utils/crm';
import type {
  ApiMeta,
  CrmLead,
  LeadActivity,
  LeadActivityCreateModel,
  LeadAssignModel,
  LeadConvertModel,
  LeadCreateModel,
  LeadFollowUpModel,
  LeadQuery,
  LeadStatus,
  LeadStatusUpdateModel,
  LeadUpdateModel,
  LeadViewMode
} from '@/types/crm/lead';

const VIEW_MODE_KEY = 'realsync.crm.leads.viewMode';

const defaultQuery: LeadQuery = {
  page: 1,
  pageSize: 10,
  search: '',
  status: null,
  priority: null,
  sourceChannel: null,
  assignedToId: null,
  minScore: null,
  maxScore: null,
  overdueFollowUp: null,
  followUpState: 'all',
  sortBy: 'createdAt',
  sortDirection: 'desc'
};

function initialViewMode(): LeadViewMode {
  const saved = localStorage.getItem(VIEW_MODE_KEY);
  return saved === 'kanban' ? 'kanban' : 'table';
}

export const useLeadStore = defineStore('lead', () => {
  const items = ref<CrmLead[]>([]);
  const selectedLead = ref<CrmLead | null>(null);
  const query = ref<LeadQuery>({ ...defaultQuery });
  const pagination = ref<ApiMeta>({
    page: defaultQuery.page,
    pageSize: defaultQuery.pageSize,
    totalCount: 0,
    totalPages: 1
  });
  const loading = ref(false);
  const detailLoading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);
  const viewMode = ref<LeadViewMode>(initialViewMode());

  const allMockItems = computed(() => {
    const raw = localStorage.getItem('realsync.crm.mock.leads');
    if (!raw) return mockLeads;

    try {
      return JSON.parse(raw) as CrmLead[];
    } catch {
      return mockLeads;
    }
  });

  const stats = computed(() => {
    const leads = allMockItems.value;
    const total = leads.length;
    const hot = leads.filter((lead) => getLeadTemperature(lead.score) === 'Hot').length;
    const overdue = leads.filter((lead) => getFollowUpState(lead.nextFollowUpAt, lead.status) === 'overdue').length;
    const won = leads.filter((lead) => lead.status === 'Won').length;

    return {
      total,
      hot,
      overdue,
      successRate: total === 0 ? 0 : Math.round((won / total) * 100)
    };
  });

  const kanbanItems = computed<Record<LeadStatus, CrmLead[]>>(() => {
    const groups: Record<LeadStatus, CrmLead[]> = {
      New: [],
      Contacted: [],
      Qualified: [],
      Proposal: [],
      Won: [],
      Lost: []
    };

    for (const lead of items.value) {
      groups[lead.status].push(lead);
    }

    return groups;
  });

  function setSelectedLead(lead: CrmLead) {
    selectedLead.value = lead;
    const index = items.value.findIndex((item) => item.id === lead.id);
    if (index >= 0) items.value[index] = lead;
  }

  function captureError(value: unknown) {
    error.value = value instanceof Error ? value.message : 'Đã có lỗi xảy ra.';
  }

  async function fetchLeads() {
    loading.value = true;
    error.value = null;
    try {
      const response = await crmLeadMockService.getLeads(query.value);
      items.value = response.data;
      if (response.meta) pagination.value = response.meta;
    } catch (err) {
      captureError(err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchLeadById(id: string) {
    detailLoading.value = true;
    error.value = null;
    try {
      const response = await crmLeadMockService.getLeadById(id);
      selectedLead.value = response.data;
      return response.data;
    } catch (err) {
      captureError(err);
      selectedLead.value = null;
      return null;
    } finally {
      detailLoading.value = false;
    }
  }

  async function createLead(payload: LeadCreateModel) {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.createLead(payload);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function updateLead(id: string, payload: LeadUpdateModel) {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.updateLead(id, payload);
      setSelectedLead(response.data);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function deleteLead(id: string) {
    submitting.value = true;
    try {
      await crmLeadMockService.deleteLead(id);
      if (selectedLead.value?.id === id) selectedLead.value = null;
      await fetchLeads();
    } finally {
      submitting.value = false;
    }
  }

  async function updateStatus(id: string, payload: LeadStatusUpdateModel) {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.updateStatus(id, payload);
      setSelectedLead(response.data);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function assignLead(id: string, payload: LeadAssignModel) {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.assignLead(id, payload);
      setSelectedLead(response.data);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function addActivity(id: string, payload: LeadActivityCreateModel): Promise<LeadActivity> {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.addActivity(id, payload);
      await fetchLeadById(id);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function setFollowUp(id: string, payload: LeadFollowUpModel) {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.setFollowUp(id, payload);
      setSelectedLead(response.data);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function convertToCustomer(id: string, payload: LeadConvertModel) {
    submitting.value = true;
    try {
      const response = await crmLeadMockService.convertToCustomer(id, payload);
      setSelectedLead(response.data);
      await fetchLeads();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  function setQuery(partial: Partial<LeadQuery>) {
    query.value = {
      ...query.value,
      ...partial,
      page: partial.page ?? 1
    };
  }

  function resetFilters() {
    query.value = { ...defaultQuery };
  }

  function setViewMode(mode: LeadViewMode) {
    viewMode.value = mode;
    localStorage.setItem(VIEW_MODE_KEY, mode);
  }

  return {
    items,
    selectedLead,
    query,
    pagination,
    loading,
    detailLoading,
    submitting,
    error,
    viewMode,
    stats,
    kanbanItems,
    fetchLeads,
    fetchLeadById,
    createLead,
    updateLead,
    deleteLead,
    updateStatus,
    assignLead,
    addActivity,
    setFollowUp,
    convertToCustomer,
    setQuery,
    resetFilters,
    setViewMode
  };
});
