export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Proposal' | 'Won' | 'Lost';

export type LeadPriority = 'Low' | 'Normal' | 'High' | 'Urgent';

export type LeadSourceChannel = 'Website' | 'Facebook' | 'Zalo' | 'Phone' | 'Referral' | 'Other';

export type LeadActivityType =
  | 'Call'
  | 'Email'
  | 'Meeting'
  | 'Note'
  | 'StatusChange'
  | 'Assigned'
  | 'FollowUp'
  | 'Converted';

export type LeadTemperature = 'Hot' | 'Warm' | 'Cold';

export type LeadFollowUpState = 'all' | 'overdue' | 'today' | 'upcoming' | 'none';

export type LeadViewMode = 'table' | 'kanban';

export interface LeadActivity {
  id: string;
  leadId: string;
  activityType: LeadActivityType;
  description?: string | null;
  oldValue?: string | null;
  newValue?: string | null;
  performedById?: string | null;
  performedByName?: string | null;
  createdAt: string;
}

export interface CrmLead {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  status: LeadStatus;
  priority: LeadPriority;
  score: number;
  interestedPropertyId?: string | null;
  interestedPropertyTitle?: string | null;
  budget?: number | null;
  requirements?: string | null;
  preferredArea?: string | null;
  preferredType?: string | null;
  assignedToId?: string | null;
  assignedToName?: string | null;
  sourceChannel?: LeadSourceChannel | null;
  lastContactedAt?: string | null;
  nextFollowUpAt?: string | null;
  convertedAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  activities: LeadActivity[];
}

export interface LeadQuery {
  page: number;
  pageSize: number;
  search: string;
  status?: LeadStatus | null;
  priority?: LeadPriority | null;
  sourceChannel?: LeadSourceChannel | null;
  assignedToId?: string | null;
  minScore?: number | null;
  maxScore?: number | null;
  overdueFollowUp?: boolean | null;
  followUpState?: LeadFollowUpState;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface LeadCreateModel {
  fullName: string;
  phone?: string | null;
  email?: string | null;
  status?: LeadStatus;
  priority?: LeadPriority;
  score?: number;
  interestedPropertyId?: string | null;
  interestedPropertyTitle?: string | null;
  budget?: number | null;
  requirements?: string | null;
  preferredArea?: string | null;
  preferredType?: string | null;
  assignedToId?: string | null;
  sourceChannel?: LeadSourceChannel | null;
  lastContactedAt?: string | null;
  nextFollowUpAt?: string | null;
}

export interface LeadUpdateModel extends LeadCreateModel {}

export interface LeadStatusUpdateModel {
  status: LeadStatus;
  note?: string | null;
}

export interface LeadAssignModel {
  assignedToId: string;
  note?: string | null;
}

export interface LeadActivityCreateModel {
  activityType: Extract<LeadActivityType, 'Call' | 'Email' | 'Meeting' | 'Note'>;
  description?: string | null;
}

export interface LeadFollowUpModel {
  nextFollowUpAt: string;
  note?: string | null;
}

export interface LeadConvertModel {
  address?: string | null;
  company?: string | null;
  notes?: string | null;
  source?: string | null;
}

export interface CrmUserMock {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Sales' | 'Agent';
  isActive: boolean;
}

export interface ApiMeta {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T;
  meta?: ApiMeta;
}

export const LEAD_STATUSES: LeadStatus[] = ['New', 'Contacted', 'Qualified', 'Proposal', 'Won', 'Lost'];
export const LEAD_PRIORITIES: LeadPriority[] = ['Low', 'Normal', 'High', 'Urgent'];
export const LEAD_SOURCE_CHANNELS: LeadSourceChannel[] = ['Website', 'Facebook', 'Zalo', 'Phone', 'Referral', 'Other'];
export const USER_ACTIVITY_TYPES: LeadActivityCreateModel['activityType'][] = ['Call', 'Email', 'Meeting', 'Note'];
