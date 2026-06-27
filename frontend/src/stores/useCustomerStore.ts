import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { mockCustomers } from '@/mocks/crm/customers';
import { crmCustomerMockService } from '@/services/mock/crmCustomerMockService';
import type {
  CrmCustomerDetail,
  CustomerActivityLog,
  CustomerAssignmentModel,
  CustomerCreateModel,
  CustomerQuery,
  CustomerUpdateModel
} from '@/types/crm/customer';
import type { ApiMeta } from '@/types/crm/lead';

const defaultQuery: CustomerQuery = {
  page: 1,
  pageSize: 10,
  search: '',
  source: null,
  assignedToId: null,
  convertedFromLeadId: null,
  convertedFromLead: null,
  origin: 'all',
  fromDate: null,
  toDate: null,
  sortBy: 'createdAt',
  sortDirection: 'desc'
};

function isSameQuery(left: CustomerQuery, right: CustomerQuery): boolean {
  return JSON.stringify(left) === JSON.stringify(right);
}

function readAllCustomers(): CrmCustomerDetail[] {
  const raw = localStorage.getItem('realsync.crm.mock.customers');
  if (!raw) return mockCustomers;

  try {
    return JSON.parse(raw) as CrmCustomerDetail[];
  } catch {
    return mockCustomers;
  }
}

export const useCustomerStore = defineStore('customer', () => {
  const items = ref<CrmCustomerDetail[]>([]);
  const selectedCustomer = ref<CrmCustomerDetail | null>(null);
  const activities = ref<CustomerActivityLog[]>([]);
  const query = ref<CustomerQuery>({ ...defaultQuery });
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

  const stats = computed(() => {
    const customers = readAllCustomers();
    const total = customers.length;
    const converted = customers.filter((customer) => !!customer.convertedFromLeadId).length;
    const direct = total - converted;
    const unassigned = customers.filter((customer) => !customer.assignedToId).length;
    return { total, converted, direct, unassigned };
  });

  function captureError(value: unknown) {
    error.value = value instanceof Error ? value.message : 'Đã có lỗi xảy ra.';
  }

  function setSelected(customer: CrmCustomerDetail) {
    selectedCustomer.value = customer;
    const index = items.value.findIndex((item) => item.id === customer.id);
    if (index >= 0) items.value[index] = customer;
    activities.value = customer.activities;
  }

  async function fetchCustomers() {
    loading.value = true;
    error.value = null;
    try {
      const response = await crmCustomerMockService.getCustomers(query.value);
      items.value = response.data;
      if (response.meta) pagination.value = response.meta;
    } catch (err) {
      captureError(err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchCustomerById(id: string) {
    detailLoading.value = true;
    error.value = null;
    try {
      const response = await crmCustomerMockService.getCustomerById(id);
      setSelected(response.data);
      return response.data;
    } catch (err) {
      captureError(err);
      selectedCustomer.value = null;
      return null;
    } finally {
      detailLoading.value = false;
    }
  }

  async function createCustomer(payload: CustomerCreateModel) {
    submitting.value = true;
    try {
      const response = await crmCustomerMockService.createCustomer(payload);
      await fetchCustomers();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function updateCustomer(id: string, payload: CustomerUpdateModel) {
    submitting.value = true;
    try {
      const response = await crmCustomerMockService.updateCustomer(id, payload);
      setSelected(response.data);
      await fetchCustomers();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function deleteCustomer(id: string) {
    submitting.value = true;
    try {
      await crmCustomerMockService.deleteCustomer(id);
      if (selectedCustomer.value?.id === id) selectedCustomer.value = null;
      await fetchCustomers();
    } finally {
      submitting.value = false;
    }
  }

  async function assignCustomer(id: string, payload: CustomerAssignmentModel) {
    submitting.value = true;
    try {
      const response = await crmCustomerMockService.assignCustomer(id, payload);
      setSelected(response.data);
      await fetchCustomers();
      return response.data;
    } finally {
      submitting.value = false;
    }
  }

  async function fetchActivities(id: string) {
    const response = await crmCustomerMockService.getCustomerActivities(id);
    activities.value = response.data;
    return response.data;
  }

  function setQuery(partial: Partial<CustomerQuery>) {
    const nextQuery = {
      ...query.value,
      ...partial,
      page: partial.page ?? 1
    };

    if (isSameQuery(query.value, nextQuery)) return;
    query.value = nextQuery;
  }

  function resetFilters() {
    query.value = { ...defaultQuery };
  }

  return {
    items,
    selectedCustomer,
    activities,
    query,
    pagination,
    loading,
    detailLoading,
    submitting,
    error,
    stats,
    fetchCustomers,
    fetchCustomerById,
    createCustomer,
    updateCustomer,
    deleteCustomer,
    assignCustomer,
    fetchActivities,
    setQuery,
    resetFilters
  };
});
