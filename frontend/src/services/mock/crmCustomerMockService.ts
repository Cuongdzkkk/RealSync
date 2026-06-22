import { mockCustomers } from '@/mocks/crm/customers';
import { mockCrmUsers } from '@/mocks/crm/users';
import type {
  CrmCustomerDetail,
  CustomerActivityLog,
  CustomerAssignmentModel,
  CustomerCreateModel,
  CustomerQuery,
  CustomerUpdateModel
} from '@/types/crm/customer';
import type { ApiResponse } from '@/types/crm/lead';

const STORAGE_KEY = 'realsync.crm.mock.customers';

function clone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value)) as T;
}

function delay() {
  const ms = 200 + Math.floor(Math.random() * 300);
  return new Promise((resolve) => window.setTimeout(resolve, ms));
}

function readDb(): CrmCustomerDetail[] {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockCustomers));
    return clone(mockCustomers);
  }

  try {
    return JSON.parse(raw) as CrmCustomerDetail[];
  } catch {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockCustomers));
    return clone(mockCustomers);
  }
}

function writeDb(customers: CrmCustomerDetail[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(customers));
}

function ok<T>(data: T, message = 'Thành công'): ApiResponse<T> {
  return { success: true, statusCode: 200, message, data };
}

function created<T>(data: T): ApiResponse<T> {
  return { success: true, statusCode: 201, message: 'Tạo thành công', data };
}

function findCustomer(customers: CrmCustomerDetail[], id: string): CrmCustomerDetail {
  const customer = customers.find((item) => item.id === id);
  if (!customer) throw new Error('Không tìm thấy khách hàng.');
  return customer;
}

function validateContact(payload: CustomerCreateModel | CustomerUpdateModel) {
  if (!payload.fullName.trim()) throw new Error('Tên khách hàng không được để trống.');
  if (payload.fullName.length > 200) throw new Error('Tên khách hàng tối đa 200 ký tự.');
  if (!payload.phone && !payload.email) throw new Error('Vui lòng nhập số điện thoại hoặc email.');
  if (payload.phone && payload.phone.length > 20) throw new Error('Số điện thoại tối đa 20 ký tự.');
  if (payload.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(payload.email)) throw new Error('Email không hợp lệ.');
  if (payload.email && payload.email.length > 200) throw new Error('Email tối đa 200 ký tự.');
  if (payload.address && payload.address.length > 500) throw new Error('Địa chỉ tối đa 500 ký tự.');
  if (payload.company && payload.company.length > 200) throw new Error('Công ty tối đa 200 ký tự.');
  if (payload.notes && payload.notes.length > 2000) throw new Error('Ghi chú tối đa 2000 ký tự.');
  if (payload.source && payload.source.length > 50) throw new Error('Nguồn tối đa 50 ký tự.');
}

function makeActivity(
  customer: CrmCustomerDetail,
  action: CustomerActivityLog['action'],
  description: string,
  oldValues?: object | null,
  newValues?: object | null
): CustomerActivityLog {
  return {
    id: `customer-activity-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
    userId: customer.assignedToId ?? 'user-admin',
    userName: customer.assignedToName ?? 'Nguyễn Minh Admin',
    entityType: 'Customer',
    entityId: customer.id,
    action,
    description,
    oldValues: oldValues ? JSON.stringify(oldValues) : null,
    newValues: newValues ? JSON.stringify(newValues) : null,
    createdAt: new Date().toISOString()
  };
}

function normalizeCustomer(payload: CustomerCreateModel | CustomerUpdateModel, current?: CrmCustomerDetail): CrmCustomerDetail {
  const assignedUser = payload.assignedToId
    ? mockCrmUsers.find((user) => user.id === payload.assignedToId)
    : null;

  if (payload.assignedToId && !assignedUser?.isActive) {
    throw new Error('Người phụ trách không hợp lệ hoặc đã ngưng hoạt động.');
  }

  return {
    id: current?.id ?? `customer-${Date.now()}`,
    fullName: payload.fullName.trim(),
    phone: payload.phone || null,
    email: payload.email || null,
    address: payload.address || null,
    company: payload.company || null,
    notes: payload.notes || null,
    source: payload.source ?? current?.source ?? 'Other',
    assignedToId: payload.assignedToId ?? current?.assignedToId ?? null,
    assignedToName: assignedUser?.fullName ?? current?.assignedToName ?? null,
    convertedFromLeadId: current?.convertedFromLeadId ?? null,
    convertedFromLeadName: current?.convertedFromLeadName ?? null,
    createdAt: current?.createdAt ?? new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    activities: current?.activities ?? []
  };
}

function applyQuery(customers: CrmCustomerDetail[], query: CustomerQuery): CrmCustomerDetail[] {
  let output = [...customers];
  const keyword = query.search.trim().toLowerCase();

  if (keyword) {
    output = output.filter((customer) =>
      [customer.fullName, customer.phone, customer.email, customer.company, customer.source]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(keyword))
    );
  }

  if (query.source) output = output.filter((customer) => customer.source === query.source);
  if (query.assignedToId) output = output.filter((customer) => customer.assignedToId === query.assignedToId);
  if (query.convertedFromLeadId) output = output.filter((customer) => customer.convertedFromLeadId === query.convertedFromLeadId);
  if (query.convertedFromLead === true) output = output.filter((customer) => !!customer.convertedFromLeadId);
  if (query.convertedFromLead === false) output = output.filter((customer) => !customer.convertedFromLeadId);
  if (query.origin === 'converted') output = output.filter((customer) => !!customer.convertedFromLeadId);
  if (query.origin === 'direct') output = output.filter((customer) => !customer.convertedFromLeadId);
  if (query.fromDate) output = output.filter((customer) => new Date(customer.createdAt) >= new Date(query.fromDate!));
  if (query.toDate) output = output.filter((customer) => new Date(customer.createdAt) <= new Date(query.toDate!));

  const sortBy = query.sortBy ?? 'createdAt';
  const direction = query.sortDirection === 'asc' ? 1 : -1;
  output.sort((a, b) => String(a[sortBy] ?? '').localeCompare(String(b[sortBy] ?? ''), 'vi') * direction);

  return output;
}

export const crmCustomerMockService = {
  resetCustomerMocks() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockCustomers));
  },

  async getCustomers(query: CustomerQuery) {
    await delay();
    const filtered = applyQuery(readDb(), query);
    const totalCount = filtered.length;
    const start = (query.page - 1) * query.pageSize;
    const data = filtered.slice(start, start + query.pageSize);

    return {
      ...ok(clone(data)),
      meta: {
        page: query.page,
        pageSize: query.pageSize,
        totalCount,
        totalPages: Math.max(1, Math.ceil(totalCount / query.pageSize))
      }
    };
  },

  async getCustomerById(id: string) {
    await delay();
    return ok(clone(findCustomer(readDb(), id)));
  },

  async createCustomer(payload: CustomerCreateModel) {
    await delay();
    validateContact(payload);
    const customers = readDb();
    const customer = normalizeCustomer(payload);
    customer.activities.unshift(makeActivity(customer, 'Create', 'Tạo khách hàng trực tiếp.', null, { fullName: customer.fullName }));
    customers.unshift(customer);
    writeDb(customers);
    return created(clone(customer));
  },

  async updateCustomer(id: string, payload: CustomerUpdateModel) {
    await delay();
    validateContact(payload);
    const customers = readDb();
    const index = customers.findIndex((customer) => customer.id === id);
    if (index < 0) throw new Error('Không tìm thấy khách hàng.');
    const before = customers[index];
    const updated = normalizeCustomer(payload, before);
    updated.activities.unshift(makeActivity(updated, 'Update', 'Cập nhật hồ sơ khách hàng.', before, payload));
    customers[index] = updated;
    writeDb(customers);
    return ok(clone(updated));
  },

  async deleteCustomer(id: string) {
    await delay();
    const customers = readDb();
    findCustomer(customers, id);
    writeDb(customers.filter((customer) => customer.id !== id));
    return ok(true, 'Xóa thành công');
  },

  async assignCustomer(id: string, payload: CustomerAssignmentModel) {
    await delay();
    const user = mockCrmUsers.find((item) => item.id === payload.assignedToId && item.isActive);
    if (!user) throw new Error('Người phụ trách không hợp lệ hoặc đã ngưng hoạt động.');
    const customers = readDb();
    const customer = findCustomer(customers, id);
    const oldValue = customer.assignedToName;
    customer.assignedToId = user.id;
    customer.assignedToName = user.fullName;
    customer.updatedAt = new Date().toISOString();
    customer.activities.unshift(
      makeActivity(customer, 'Assignment', payload.note || `Phân công khách hàng cho ${user.fullName}.`, { assignedToName: oldValue }, { assignedToName: user.fullName })
    );
    writeDb(customers);
    return ok(clone(customer));
  },

  async getCustomerActivities(id: string) {
    await delay();
    const customer = findCustomer(readDb(), id);
    return ok(clone(customer.activities));
  }
};
