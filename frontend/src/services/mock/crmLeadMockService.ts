import type {
  ApiResponse,
  CrmLead,
  LeadActivity,
  LeadActivityCreateModel,
  LeadAssignModel,
  LeadConvertModel,
  LeadCreateModel,
  LeadFollowUpModel,
  LeadQuery,
  LeadStatusUpdateModel,
  LeadUpdateModel
} from '@/types/crm/lead';
import { LEAD_STATUSES } from '@/types/crm/lead';
import { getFollowUpState } from '@/utils/crm';
import { mockLeads } from '@/mocks/crm/leads';
import { mockCrmUsers } from '@/mocks/crm/users';

const STORAGE_KEY = 'realsync.crm.mock.leads';

function clone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value)) as T;
}

function delay() {
  const ms = 200 + Math.floor(Math.random() * 300);
  return new Promise((resolve) => window.setTimeout(resolve, ms));
}

function readDb(): CrmLead[] {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockLeads));
    return clone(mockLeads);
  }

  try {
    return JSON.parse(raw) as CrmLead[];
  } catch {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockLeads));
    return clone(mockLeads);
  }
}

function writeDb(leads: CrmLead[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(leads));
}

function ok<T>(data: T, message = 'Thành công'): ApiResponse<T> {
  return { success: true, statusCode: 200, message, data };
}

function created<T>(data: T): ApiResponse<T> {
  return { success: true, statusCode: 201, message: 'Tạo thành công', data };
}

function findLead(leads: CrmLead[], id: string): CrmLead {
  const lead = leads.find((item) => item.id === id);
  if (!lead) throw new Error('Không tìm thấy Lead.');
  return lead;
}

function makeActivity(
  lead: CrmLead,
  activityType: LeadActivity['activityType'],
  description: string,
  oldValue?: string | null,
  newValue?: string | null
): LeadActivity {
  return {
    id: `activity-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
    leadId: lead.id,
    activityType,
    description,
    oldValue,
    newValue,
    performedById: lead.assignedToId ?? 'user-admin',
    performedByName: lead.assignedToName ?? 'Nguyễn Minh Admin',
    createdAt: new Date().toISOString()
  };
}

function applyQuery(leads: CrmLead[], query: LeadQuery): CrmLead[] {
  let output = [...leads];
  const keyword = query.search.trim().toLowerCase();

  if (keyword) {
    output = output.filter((lead) =>
      [
        lead.fullName,
        lead.phone,
        lead.email,
        lead.requirements,
        lead.preferredArea,
        lead.preferredType
      ]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(keyword))
    );
  }

  if (query.status) output = output.filter((lead) => lead.status === query.status);
  if (query.priority) output = output.filter((lead) => lead.priority === query.priority);
  if (query.sourceChannel) output = output.filter((lead) => lead.sourceChannel === query.sourceChannel);
  if (query.assignedToId) output = output.filter((lead) => lead.assignedToId === query.assignedToId);
  if (query.minScore !== null && query.minScore !== undefined) output = output.filter((lead) => lead.score >= Number(query.minScore));
  if (query.maxScore !== null && query.maxScore !== undefined) output = output.filter((lead) => lead.score <= Number(query.maxScore));

  if (query.overdueFollowUp === true) {
    output = output.filter((lead) => getFollowUpState(lead.nextFollowUpAt, lead.status) === 'overdue');
  }

  if (query.followUpState && query.followUpState !== 'all') {
    output = output.filter((lead) => getFollowUpState(lead.nextFollowUpAt, lead.status) === query.followUpState);
  }

  const direction = query.sortDirection === 'asc' ? 1 : -1;
  const sortBy = query.sortBy ?? 'createdAt';
  output.sort((a, b) => {
    const av = a[sortBy as keyof CrmLead];
    const bv = b[sortBy as keyof CrmLead];
    if (typeof av === 'number' && typeof bv === 'number') return (av - bv) * direction;
    return String(av ?? '').localeCompare(String(bv ?? ''), 'vi') * direction;
  });

  return output;
}

function normalizeLead(payload: LeadCreateModel | LeadUpdateModel, current?: CrmLead): CrmLead {
  const assignedUser = payload.assignedToId
    ? mockCrmUsers.find((user) => user.id === payload.assignedToId)
    : null;

  return {
    id: current?.id ?? `lead-${Date.now()}`,
    fullName: payload.fullName.trim(),
    phone: payload.phone || null,
    email: payload.email || null,
    status: payload.status ?? current?.status ?? 'New',
    priority: payload.priority ?? current?.priority ?? 'Normal',
    score: Number(payload.score ?? current?.score ?? 40),
    interestedPropertyId: payload.interestedPropertyId ?? current?.interestedPropertyId ?? null,
    interestedPropertyTitle: payload.interestedPropertyTitle ?? current?.interestedPropertyTitle ?? null,
    budget: payload.budget ?? current?.budget ?? null,
    requirements: payload.requirements ?? current?.requirements ?? null,
    preferredArea: payload.preferredArea ?? current?.preferredArea ?? null,
    preferredType: payload.preferredType ?? current?.preferredType ?? null,
    assignedToId: payload.assignedToId ?? current?.assignedToId ?? null,
    assignedToName: assignedUser?.fullName ?? current?.assignedToName ?? null,
    sourceChannel: payload.sourceChannel ?? current?.sourceChannel ?? null,
    lastContactedAt: payload.lastContactedAt ?? current?.lastContactedAt ?? null,
    nextFollowUpAt: payload.nextFollowUpAt ?? current?.nextFollowUpAt ?? null,
    convertedAt: current?.convertedAt ?? null,
    createdAt: current?.createdAt ?? new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    activities: current?.activities ?? []
  };
}

export const crmLeadMockService = {
  resetMockDatabase() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(mockLeads));
  },

  async getLeads(query: LeadQuery) {
    await delay();
    const filtered = applyQuery(readDb(), query);
    const totalCount = filtered.length;
    const start = (query.page - 1) * query.pageSize;
    const data = filtered.slice(start, start + query.pageSize);

    return {
      ...ok(data),
      meta: {
        page: query.page,
        pageSize: query.pageSize,
        totalCount,
        totalPages: Math.max(1, Math.ceil(totalCount / query.pageSize))
      }
    };
  },

  async getLeadById(id: string) {
    await delay();
    return ok(clone(findLead(readDb(), id)));
  },

  async createLead(payload: LeadCreateModel) {
    await delay();
    const leads = readDb();
    const lead = normalizeLead(payload);
    lead.activities.unshift(makeActivity(lead, 'Note', 'Tạo Lead mới từ CRM UI.'));
    leads.unshift(lead);
    writeDb(leads);
    return created(clone(lead));
  },

  async updateLead(id: string, payload: LeadUpdateModel) {
    await delay();
    const leads = readDb();
    const index = leads.findIndex((lead) => lead.id === id);
    if (index < 0) throw new Error('Không tìm thấy Lead.');
    leads[index] = normalizeLead(payload, leads[index]);
    leads[index].activities.unshift(makeActivity(leads[index], 'Note', 'Cập nhật thông tin Lead.'));
    writeDb(leads);
    return ok(clone(leads[index]));
  },

  async deleteLead(id: string) {
    await delay();
    const leads = readDb();
    writeDb(leads.filter((lead) => lead.id !== id));
    return ok(true, 'Xóa thành công');
  },

  async updateStatus(id: string, payload: LeadStatusUpdateModel) {
    await delay();
    if (!LEAD_STATUSES.includes(payload.status)) throw new Error('Trạng thái Lead không hợp lệ.');
    const leads = readDb();
    const lead = findLead(leads, id);
    const oldStatus = lead.status;
    lead.status = payload.status;
    if (payload.status === 'Won' && !lead.convertedAt) lead.convertedAt = new Date().toISOString();
    lead.updatedAt = new Date().toISOString();
    lead.activities.unshift(makeActivity(lead, 'StatusChange', payload.note || 'Cập nhật trạng thái Lead.', oldStatus, payload.status));
    writeDb(leads);
    return ok(clone(lead));
  },

  async assignLead(id: string, payload: LeadAssignModel) {
    await delay();
    const user = mockCrmUsers.find((item) => item.id === payload.assignedToId && item.isActive);
    if (!user) throw new Error('Người phụ trách không hợp lệ hoặc đã ngưng hoạt động.');
    const leads = readDb();
    const lead = findLead(leads, id);
    const oldValue = lead.assignedToName;
    lead.assignedToId = user.id;
    lead.assignedToName = user.fullName;
    lead.updatedAt = new Date().toISOString();
    lead.activities.unshift(makeActivity(lead, 'Assigned', payload.note || `Phân công Lead cho ${user.fullName}.`, oldValue, user.fullName));
    writeDb(leads);
    return ok(clone(lead));
  },

  async addActivity(id: string, payload: LeadActivityCreateModel) {
    await delay();
    if (!['Call', 'Email', 'Meeting', 'Note'].includes(payload.activityType)) {
      throw new Error('Không thể tạo system activity từ form.');
    }
    const leads = readDb();
    const lead = findLead(leads, id);
    const activity = makeActivity(lead, payload.activityType, payload.description || 'Thêm hoạt động chăm sóc.');
    lead.activities.unshift(activity);
    if (payload.activityType !== 'Note') lead.lastContactedAt = activity.createdAt;
    lead.updatedAt = new Date().toISOString();
    writeDb(leads);
    return created(clone(activity));
  },

  async getActivities(id: string) {
    await delay();
    const lead = findLead(readDb(), id);
    return ok(clone(lead.activities));
  },

  async setFollowUp(id: string, payload: LeadFollowUpModel) {
    await delay();
    if (new Date(payload.nextFollowUpAt) <= new Date()) throw new Error('Thời gian chăm sóc tiếp theo phải ở tương lai.');
    const leads = readDb();
    const lead = findLead(leads, id);
    const oldValue = lead.nextFollowUpAt;
    lead.nextFollowUpAt = payload.nextFollowUpAt;
    lead.updatedAt = new Date().toISOString();
    lead.activities.unshift(makeActivity(lead, 'FollowUp', payload.note || 'Đặt lịch chăm sóc tiếp theo.', oldValue, payload.nextFollowUpAt));
    writeDb(leads);
    return ok(clone(lead));
  },

  async convertToCustomer(id: string, payload: LeadConvertModel) {
    await delay();
    const leads = readDb();
    const lead = findLead(leads, id);
    if (lead.convertedAt) throw new Error('Lead này đã được chuyển thành khách hàng.');
    lead.status = 'Won';
    lead.convertedAt = new Date().toISOString();
    lead.updatedAt = new Date().toISOString();
    lead.activities.unshift(makeActivity(lead, 'Converted', payload.notes || 'Đã chuyển Lead thành khách hàng.', null, 'Customer'));
    writeDb(leads);
    return created(clone(lead));
  }
};
